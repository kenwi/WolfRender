using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Threading;
using WolfRender.Interfaces;
using WolfRender.Models;
using WolfRender.Models.Configuration;
using System.Linq;

namespace WolfRender.Services
{
    internal class PlayerService : IPlayerService
    {
        public IPlayer Player => _player;
        public Sprite CurrentWeaponSprite => _currentWeaponSprite;

        private readonly IPlayer _player;
        private readonly IAnimationService _animationService;
        private readonly ILogger<PlayerService> _logger;
        private readonly IMapService _mapService;
        private readonly GameConfiguration _gameConfiguration;
        private IWindowService _windowService;
        private IMapRendererService _mapRendererService;
        
        // Movement properties
        private Vector2f velocity;
        private Vector2f acceleration;
        private readonly float maxSpeed = 16.0f;
        private readonly float accelerationRate = 25.0f;
        private readonly float friction = 8.0f;
        float _mouseSpeedMultiplier = 0.02f;
        float _deltaTime;
        Sprite _currentWeaponSprite;
        float _weaponAnimationTime;
        private IEntityService _entityService;
        private readonly ICollisionService _collisionService;

        public PlayerService(
            ILogger<PlayerService> logger,
            IMapService mapService,
            IOptions<GameConfiguration> gameConfiguration,
            IAnimationService animationService,
            ICollisionService collisionService)
        {
            _logger = logger;
            _mapService = mapService;
            _gameConfiguration = gameConfiguration.Value;
            _player = new Player();
            _animationService = animationService;
            _collisionService = collisionService;
            _logger.LogInformation("PlayerService starting");
        }

        public void Init(IWindowService windowService, IMapRendererService mapRendererService, IEntityService entityService)
        {
            _mapRendererService = mapRendererService;
            _windowService = windowService;
            _windowService.IsMouseVisible = false;
            _entityService = entityService;
            RegisterMouseWheelEvents();

            _logger.LogInformation("PlayerService initialized with window service");
        }

        public void Update(float deltaTime)
        {
            // Store delta time
            _deltaTime = deltaTime;

            // Get input
            GetKeypressInput();

            // Get movement (WASD) input
            Vector2f input = GetKeyboardInputDirection();

            // Calculate movement direction based on player's facing direction
            float cos = (float)Math.Cos(_player.Direction);
            float sin = (float)Math.Sin(_player.Direction);

            // Transform input to world space
            acceleration = new Vector2f(
                (input.X * cos - input.Y * sin) * accelerationRate,
                (input.X * sin + input.Y * cos) * accelerationRate
            );

            // Apply acceleration
            velocity.X += acceleration.X * deltaTime;
            velocity.Y += acceleration.Y * deltaTime;

            // Apply friction
            velocity.X -= velocity.X * friction * deltaTime;
            velocity.Y -= velocity.Y * friction * deltaTime;

            // Clamp velocity to maximum speed
            float speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            if (speed > maxSpeed)
            {
                velocity.X = (velocity.X / speed) * maxSpeed;
                velocity.Y = (velocity.Y / speed) * maxSpeed;
            }

            // Calculate new position
            Vector2f newPosition = new Vector2f(
                _player.Position.X + velocity.X * deltaTime,
                _player.Position.Y + velocity.Y * deltaTime
            );

            // Apply collision detection using the CollisionService
            _player.Position = _collisionService.GetValidPosition(_player.Position, newPosition);

            // Get mouse delta and rotate player
            HandleMouseRotation(deltaTime);

            HandleWeaponsShooting(deltaTime);
        }

        bool hasCheckedLineOfSight = false;
        private void HandleWeaponsShooting(float deltaTime)
        {
            if (_currentWeaponSprite == null)
            {
                _currentWeaponSprite = _animationService.GetAnimationFrame("weapons", "shoot", 0, 16);
            }

            if (Mouse.IsButtonPressed(Mouse.Button.Left))
            {
                _player.IsShooting = true;
            }

            if (_player.IsShooting)
            {
                _weaponAnimationTime += deltaTime;
                if (_weaponAnimationTime > 0.5f)
                {
                    _player.IsShooting = false;
                    _weaponAnimationTime = 0;
                    hasCheckedLineOfSight = false;
                }
                _currentWeaponSprite = _animationService.GetAnimationFrame("weapons", "shoot", _weaponAnimationTime, 16);

                if (!hasCheckedLineOfSight)
                {
                    foreach(var entity in _entityService.Entities)
                    {
                        if (entity is AnimatedEntity animatedEntity)
                        {
                            if (entity.Sprite == null || !entity.IsAlive)
                            {
                                continue;
                            }

                            var screenSpacePosition = entity.Sprite.Position;
                            //var crosshairPosition = new Vector2f(_windowService.WindowCenter.X, _windowService.WindowCenter.Y);
                            var crosshairPosition = new Vector2f(1024 / 2, 768 / 2);
                            var spriteWidth = (entity.Sprite.TextureRect.Width - 40) * entity.Sprite.Scale.X;
                            var spriteHeight = entity.Sprite.TextureRect.Height * entity.Sprite.Scale.Y;
                            
                            // Check if crosshair is within sprite bounds
                            bool isWithinBounds = 
                                crosshairPosition.X >= screenSpacePosition.X - spriteWidth/2 &&
                                crosshairPosition.X <= screenSpacePosition.X + spriteWidth/2 &&
                                crosshairPosition.Y >= screenSpacePosition.Y - spriteHeight/2 &&
                                crosshairPosition.Y <= screenSpacePosition.Y + spriteHeight/2;

                            if (isWithinBounds && _collisionService.HasLineOfSight(_player.Position, animatedEntity.Position))
                            {
                                // Hit the entity!
                                animatedEntity.StopFollowingPath();
                                animatedEntity.SetAnimation("death");
                                _logger.LogInformation($"Hit entity at screen position {screenSpacePosition}");
                                break;
                            }
                        }
                    }

                    hasCheckedLineOfSight = true;
                }
            }
        }

        private Vector2f GetKeyboardInputDirection()
        {
            Vector2f input = new Vector2f(0, 0);

            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                input.X += 1;
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                input.X -= 1;
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                input.Y += 1;
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                input.Y -= 1;

            return input;
        }

        private void RegisterMouseWheelEvents()
        {
            _windowService.Window.MouseWheelScrolled += (s, e) =>
            {
                _player.Fov += e.Delta * 0.1f;
                _player.FovHalf = _player.Fov * 0.5f;

                // Log FoV changes, convert to degrees
                _logger.LogInformation($"FoV: {_player.Fov * 180 / MathF.PI}° {_player.Fov} {_player.FovHalf}");


            };
            _windowService.Window.MouseButtonPressed += (s, e) =>
            {
                if (e.Button == Mouse.Button.Middle)
                {
                    _player.Fov = _gameConfiguration.DefaultFov;
                    _player.FovHalf = _player.Fov * 0.5f;
                }
            };
            _logger.BeginScope("Mouse wheel events registered");
        }

        private void HandleMouseRotation(float deltaTime)
        {
            if (_windowService.IsMouseVisible)
            {
                return;
            }

            var mouseDelta = Mouse.GetPosition() - _windowService.WindowCenter;
            if (mouseDelta.X != 0)
            {
                _player.Direction += _player.RotationSpeed * mouseDelta.X * _mouseSpeedMultiplier * deltaTime;
            }

            if (!_windowService.IsMouseVisible)
            {
                Mouse.SetPosition(_windowService.WindowCenter);
            }
        }

        private void GetKeypressInput()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
            {
                _windowService.StopAsync(CancellationToken.None);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.M))
            {
                _windowService.IsMouseVisible = !_windowService.IsMouseVisible;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                _gameConfiguration.ShadingExponent += 10.0f * _deltaTime;
                _mapRendererService.CalculateZBuffer();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                if (_gameConfiguration.ShadingExponent <= 0.01)
                {
                    return;
                }
                _gameConfiguration.ShadingExponent -= 10.0f * _deltaTime;
                _mapRendererService.CalculateZBuffer();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Home))
            {
                _gameConfiguration.ShadingExponent = 5;
                _mapRendererService.CalculateZBuffer();
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.R))
            {
                _entityService.Entities.ForEach(entity =>
                {
                    if (entity is AnimatedEntity animatedEntity)
                    {
                        animatedEntity.IsAlive = true;
                        animatedEntity.SetAnimation("idle");
                    }
                });
            }
        }
    }
}
