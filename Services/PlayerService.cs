using Microsoft.Extensions.Logging;
using SFML.System;
using SFML.Window;
using System;
using System.Threading;
using WolfRender.Interfaces;
using WolfRender.Models;

namespace WolfRender.Services
{
    internal class PlayerService : IPlayerService
    {
        public IPlayer Player => _player;

        private readonly IPlayer _player;
        private readonly ILogger<PlayerService> _logger;
        private readonly IMapService _mapService;
        private IWindowService _windowService;

        // Movement properties
        private Vector2f velocity;
        private Vector2f acceleration;
        private readonly float maxSpeed = 16.0f;
        private readonly float accelerationRate = 25.0f;
        private readonly float friction = 8.0f;
        float _mouseSpeedMultiplier = 0.02f;

        public PlayerService(
            ILogger<PlayerService> logger,
            IMapService mapService)
        {
            _logger = logger;
            _mapService = mapService;
            _player = new Player();
            _logger.LogInformation("PlayerService starting");
        }

        public void Init(IWindowService windowService)
        {
            _windowService = windowService;
            _windowService.IsMouseVisible = false;
            _logger.LogInformation("PlayerService initialized with window service");
        }

        public void Update(float deltaTime)
        {
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

            // Collision detection with walls
            HandleCollision(newPosition);

            // Get mouse delta and rotate player
            HandleMouseRotation(deltaTime);
        }

        private Vector2f GetKeyboardInputDirection()
        {
            Vector2f input = new Vector2f(0, 0);

            if (Input.IsKeyPressed(Keyboard.Key.W))
                input.X += 1;
            if (Input.IsKeyPressed(Keyboard.Key.S))
                input.X -= 1;
            if (Input.IsKeyPressed(Keyboard.Key.D))
                input.Y += 1;
            if (Input.IsKeyPressed(Keyboard.Key.A))
                input.Y -= 1;

            return input;
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
            if (Input.IsKeyPressed(Keyboard.Key.Escape))
            {
                _windowService.StopAsync(CancellationToken.None);
            }

            if (Input.IsKeyPressed(Keyboard.Key.M))
            {
                _windowService.IsMouseVisible = !_windowService.IsMouseVisible;
            }
        }

        private void HandleCollision(Vector2f newPosition)
        {
            if (_mapService.Get(new Vector2i((int)newPosition.X, (int)_player.Position.Y)) == 0)
                _player.Position = new Vector2f(newPosition.X, _player.Position.Y);

            if (_mapService.Get(new Vector2i((int)_player.Position.X, (int)newPosition.Y)) == 0)
                _player.Position = new Vector2f(_player.Position.X, newPosition.Y);
        }
    }
}
