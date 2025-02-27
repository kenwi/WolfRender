using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;
using WolfRender.Models;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class SpriteRenderService : Drawable, ISpriteRendererService, IUpdateable
    {
        private readonly ILogger<SpriteRenderService> _logger;
        private readonly IPlayerService _playerService;
        private readonly IPlayer _player;
        private readonly ITextureService _textureService;
        private readonly IAnimationService _animationService;
        private readonly GameConfiguration _gameConfiguration;
        private readonly IMapService _mapService;
        private List<Vector2f> _spritePositions;
        private int _resolutionX;
        private int _resolutionY;
        private Vector2f _guardPosition;
        private Texture _barrelTexture;
        private Sprite _barrelSprite;
        private double[] _wallDistances;
        private List<IEntity> _entities;


        public SpriteRenderService(
            IPlayerService playerService,
            ILogger<SpriteRenderService> logger,
            ITextureService textureService,
            IAnimationService animationService,
            IMapService mapService,
            IOptions<GameConfiguration> gameConfiguration)
        {
            _logger = logger;
            _playerService = playerService;
            _player = _playerService.Player;
            _textureService = textureService;
            _animationService = animationService;
            _gameConfiguration = gameConfiguration.Value;
            _mapService = mapService;
            _logger.LogInformation("SpriteRenderService starting");
        }
        
        public void Init()
        {
            // Load guard sprite sheet
            _animationService.LoadSpriteSheet("guard", "Assets/enemy_guard.png");
            
            // Create idle animation (first row)
            _animationService.CreateAnimation("guard", "idle", 0, 8);
            
            // Create walk animation (rows 1-4)
            _animationService.CreateMultiRowAnimation("guard", "walk", 1, 4, 8);

            // Create attack animation (row 6, 3 frames)
            _animationService.CreateMultiRowAnimation("guard", "attack", 6, 1, 4);

            // Create death animation (row 7, 8 frames)
            _animationService.CreateMultiRowAnimation("guard", "death", 5, 1, 5);


            // Create a new sprite with transparency
            _textureService.LoadTexture("barrel", "Assets/barrel.png");
            var barrelImage = _textureService.GetTextureImage("barrel");
            barrelImage.CreateMaskFromColor(Color.Black);
            _barrelTexture = new Texture(barrelImage);
            _barrelSprite = new Sprite(_barrelTexture);
            _barrelSprite.Position = new Vector2f(20.0f, 45.5f);
            _barrelSprite.Origin = new Vector2f(_barrelTexture.Size.X / 2, _barrelTexture.Size.Y / 2);


            _guardPosition = new Vector2f(20.5f, 45.5f);
            _spritePositions = new List<Vector2f>
            {
                new Vector2f(20.5f, 46.5f),
                new Vector2f(20.5f, 44.5f),
                _guardPosition  // Add guard position
            };

            _resolutionX = _gameConfiguration.Resolution.X;
            _resolutionY = _gameConfiguration.Resolution.Y;

            _entities = new List<IEntity>
            {
                new StaticEntity(_textureService, "barrel")
                {
                    Position = new Vector2f(20.5f, 46.5f),
                    Direction = 0
                },
                new StaticEntity(_textureService, "barrel")
                {
                    Position = new Vector2f(20.5f, 44.5f),
                    Direction = 0
                }
            };

            // Create guard entity
            var guard = new AnimatedEntity(_animationService, "guard")
            {
                Position = _guardPosition,
                Direction = 0
            };
            
            // Set initial animation
            guard.SetAnimation("idle");
            
            // Add to entities list
            _entities.Add(guard);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _wallDistances = _mapService.WallDistances;
            
            foreach(var entity in _entities)
            {
                switch(entity)
                {
                    case StaticEntity staticEntity:
                        RenderStaticSprite(target, states, staticEntity);
                        break;
                    case AnimatedEntity animatedEntity:
                        RenderAnimatedEntity(target, states, animatedEntity);
                        break;
                }
            }
        }

        bool isAnimating = false;
        public void Update(float deltaTime)
        {
            // Update all entities
            foreach (var entity in _entities)
            {
                if (entity is AnimatedEntity animatedEntity)
                {
                    animatedEntity.Update(deltaTime);

                    if (isAnimating)
                        break;

                    // Example: Switch between idle and walk animations
                    if (Input.IsKeyPressed(Keyboard.Key.E))
                    {
                        animatedEntity.SetAnimation("walk");
                    }
                    else
                    {
                        animatedEntity.SetAnimation("idle");
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.Q))
                    {
                        animatedEntity.SetAnimation("attack");
                        isAnimating = true;
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.T))
                    {
                        animatedEntity.SetAnimation("death");
                        isAnimating = true;
                    }
                }
            }
        }

        private void RenderAnimatedEntity(RenderTarget target, RenderStates states, AnimatedEntity entity)
        {
            // Calculate vector from player to entity
            double spriteX = entity.Position.X - _player.Position.X;
            double spriteY = entity.Position.Y - _player.Position.Y;
            
            // Calculate angle between player and entity
            double playerToEntityAngle = Math.Atan2(spriteY, spriteX);
            
            // Normalize to [0, 2π)
            while (playerToEntityAngle < 0) playerToEntityAngle += 2 * Math.PI;
            while (playerToEntityAngle >= 2 * Math.PI) playerToEntityAngle -= 2 * Math.PI;
            
            // Calculate sprite index (8 directions)
            int spriteIndex = (int)Math.Round(playerToEntityAngle / (Math.PI * 2) * 8) % 8;
            
            // Adjust the index to match the sprite sheet orientation
            spriteIndex = (8 - spriteIndex) % 8;
            spriteIndex = (spriteIndex + 4) % 8;
            
            // Get the appropriate sprite based on animation, angle, and time
            Sprite entitySprite;
            if (entity.GetCurrentAnimation() == "idle")
            {
                entitySprite = _animationService.GetSprite(entity.SheetName, "idle", spriteIndex);
            }
            else if (entity.GetCurrentAnimation() == "attack")
            {
                entitySprite = _animationService.GetAnimationFrame(
                    entity.SheetName,
                    entity.GetCurrentAnimation(),
                    entity.GetAnimationTime(),
                    entity.GetFrameRate());
                
                if(_animationService.CurrentFrameIndex == 3)
                {
                    isAnimating = false;
                    entity.SetAnimation("idle");
                }
            }
            else if (entity.GetCurrentAnimation() == "death")
            {
                entitySprite = _animationService.GetAnimationFrame(
                    entity.SheetName,
                    entity.GetCurrentAnimation(),
                    entity.GetAnimationTime(),
                    entity.GetFrameRate());

                if (_animationService.CurrentFrameIndex == 4)
                {
                    isAnimating = false;
                }
            }
            else
            {
                entitySprite = _animationService.GetAnimationFrame(
                    entity.SheetName, 
                    entity.GetCurrentAnimation(), 
                    spriteIndex, 
                    entity.GetAnimationTime(), 
                    entity.GetFrameRate());
            }
            
            // Calculate relative angle for screen positioning
            double relativeAngle = playerToEntityAngle - _player.Direction;

            // Normalize angle to [-PI, PI]
            while (relativeAngle > Math.PI) relativeAngle -= 2 * Math.PI;
            while (relativeAngle < -Math.PI) relativeAngle += 2 * Math.PI;

            // Calculate distance for scaling
            double distance = Math.Sqrt(spriteX * spriteX + spriteY * spriteY);

            // Calculate screen position
            float screenX = _resolutionX / 2 + (float)(relativeAngle / _player.FovHalf * _resolutionX / 2);

            // Calculate scale
            float scale = _resolutionY / (float)(distance * 2) / 64; // Use 64 for sprite size

            // Calculate sprite width in screen space
            float spriteScreenWidth = _barrelTexture.Size.X * scale;

            // Calculate sprite screen bounds
            int spriteLeft = (int)(screenX - spriteScreenWidth / 2);
            int spriteRight = (int)(screenX + spriteScreenWidth / 2);

            // Clamp to screen bounds
            spriteLeft = Math.Max(0, Math.Min(spriteLeft, _resolutionX - 1));
            spriteRight = Math.Max(0, Math.Min(spriteRight, _resolutionX - 1));

            // Check if guard is occluded by walls
            bool isFullyVisible = true;
            bool isPartiallyVisible = false;

            // Create arrays to track which columns are visible
            bool[] columnVisibility = new bool[spriteRight - spriteLeft + 1];

            for (int x = spriteLeft; x <= spriteRight; x++)
            {
                int columnIndex = x - spriteLeft;
                double wallDist = _wallDistances[x];

                // Check visibility for this column
                if (distance < wallDist)
                {
                    isPartiallyVisible = true;
                    columnVisibility[columnIndex] = true;
                }
                else
                {
                    isFullyVisible = false;
                    columnVisibility[columnIndex] = false;
                }
            }

            // Determine final visibility state
            if (isFullyVisible)
            {
                // Render normally
                entitySprite.Scale = new Vector2f(scale * 2, scale * 2);
                entitySprite.Position = new Vector2f(screenX, _resolutionY / 2);
                target.Draw(entitySprite, states);
            }
            else if (isPartiallyVisible)
            {
                // Create a clipped version of the sprite
                RenderPartiallyOccludedGuard(target, spriteLeft, spriteRight, screenX, scale, entitySprite);
            }

            // For debugging
            if (Input.IsKeyPressed(Keyboard.Key.Space))
            {
                Console.WriteLine($"Player: {_player.Position}, Guard: {entity.Position}");
                Console.WriteLine($"Angle: {playerToEntityAngle * 180 / Math.PI}°, Sprite: {spriteIndex}");
            }
        }

        private void RenderStaticSprite(RenderTarget target, RenderStates states, IEntity entity)
        {
            var spritePosition = entity.Position;

            // Calculate vector from player to sprite
            double spriteX = spritePosition.X - _player.Position.X;
            double spriteY = spritePosition.Y - _player.Position.Y;

            // Calculate angle between player's direction and sprite
            double playerToSpriteAngle = Math.Atan2(spriteY, spriteX);
            double relativeAngle = playerToSpriteAngle - _player.Direction;

            // Normalize angle to [-PI, PI]
            while (relativeAngle > Math.PI) relativeAngle -= 2 * Math.PI;
            while (relativeAngle < -Math.PI) relativeAngle += 2 * Math.PI;

            // Calculate distance to sprite (for scaling)
            double distance = Math.Sqrt(spriteX * spriteX + spriteY * spriteY);

            // Calculate screen X position based on relative angle
            float screenX = _resolutionX / 2 + (float)(relativeAngle / _player.FovHalf * _resolutionX / 2);

            // Calculate sprite scale based on distance
            float scale = _resolutionY / (float)(distance * 2) / _barrelTexture.Size.Y;

            // Calculate sprite width in screen space
            float spriteScreenWidth = _barrelTexture.Size.X * scale;

            // Calculate sprite screen bounds
            int spriteLeft = (int)(screenX - spriteScreenWidth / 2);
            int spriteRight = (int)(screenX + spriteScreenWidth / 2);

            // Clamp to screen bounds
            spriteLeft = Math.Max(0, Math.Min(spriteLeft, _resolutionX - 1));
            spriteRight = Math.Max(0, Math.Min(spriteRight, _resolutionX - 1));

            // Check if sprite is occluded by walls
            bool isFullyVisible = true;
            bool isPartiallyVisible = false;

            // Create arrays to track which columns are visible
            bool[] columnVisibility = new bool[spriteRight - spriteLeft + 1];

            for (int x = spriteLeft; x <= spriteRight; x++)
            {
                int columnIndex = x - spriteLeft;
                double wallDist = _wallDistances[x];

                // Check visibility for this column
                if (distance < wallDist)
                {
                    isPartiallyVisible = true;
                    columnVisibility[columnIndex] = true;
                }
                else
                {
                    isFullyVisible = false;
                    columnVisibility[columnIndex] = false;
                }
            }

            // Determine final visibility state
            if (isFullyVisible)
            {
                // Render normally
                //_barrelSprite.Scale = new Vector2f(scale * 2, scale * 2);
                //_barrelSprite.Position = new Vector2f(screenX, _resolutionY / 2);

                entity.Sprite.Scale = new Vector2f(scale * 2, scale * 2);
                entity.Sprite.Position = new Vector2f(screenX, _resolutionY / 2);

                target.Draw(entity.Sprite, states);
                //target.Draw(_barrelSprite, states);
            }
            else if (isPartiallyVisible)
            {
                // Create a clipped version of the sprite
                RenderPartiallyOccludedSprite(target, entity.Texture, spriteLeft, spriteRight, screenX, scale, distance, columnVisibility);
            }
        }

        private void RenderPartiallyOccludedSprite(RenderTarget target, Texture texture, int spriteLeft, int spriteRight,
            float screenX, float scale, double distance, bool[] columnVisibility)
        {
            // Create a render texture the size of the sprite on screen
            int spriteWidth = spriteRight - spriteLeft + 1;
            RenderTexture renderTexture = new RenderTexture((uint)spriteWidth, (uint)_resolutionY);
            renderTexture.Clear(Color.Transparent);

            // Draw the original sprite to the render texture
            Sprite tempSprite = new Sprite(texture);
            tempSprite.Scale = new Vector2f(scale * 2, scale * 2);
            tempSprite.Position = new Vector2f(spriteWidth / 2, _resolutionY / 2);
            tempSprite.Origin = new Vector2f(_barrelTexture.Size.X / 2, _barrelTexture.Size.Y / 2);
            renderTexture.Draw(tempSprite);
            renderTexture.Display();

            // Create a mask to hide occluded parts
            for (int x = 0; x < spriteWidth; x++)
            {
                if (!columnVisibility[x])
                {
                    // Draw a vertical transparent strip to mask this column
                    RectangleShape mask = new RectangleShape(new Vector2f(1, _resolutionY));
                    mask.Position = new Vector2f(x, 0);
                    mask.FillColor = Color.Transparent;
                    renderTexture.Draw(mask, new RenderStates(BlendMode.None));
                }
            }
            renderTexture.Display();

            // Draw the clipped sprite to the main target
            Sprite clippedSprite = new Sprite(renderTexture.Texture);
            clippedSprite.Position = new Vector2f(spriteLeft, 0);
            target.Draw(clippedSprite);

            // Clean up
            renderTexture.Dispose();
            clippedSprite.Dispose();
            tempSprite.Dispose();
        }

        private void RenderPartiallyOccludedGuard(RenderTarget target, int spriteLeft, int spriteRight,
            float screenX, float scale, Sprite guardSprite)
        {
            // Create a render texture the size of the sprite on screen
            int spriteWidth = spriteRight - spriteLeft + 1;
            RenderTexture renderTexture = new RenderTexture((uint)spriteWidth, (uint)_resolutionY);
            renderTexture.Clear(Color.Transparent);  // Make sure we start with transparency

            // Draw the guard sprite to the render texture
            guardSprite.Scale = new Vector2f(scale * 2, scale * 2);
            guardSprite.Position = new Vector2f(spriteWidth / 2, _resolutionY / 2);
            renderTexture.Draw(guardSprite);
            renderTexture.Display();

            // Create a mask to hide occluded parts
            for (int x = 0; x < spriteWidth; x++)
            {
                int worldX = x + spriteLeft;
                if (worldX >= 0 && worldX < _resolutionX)
                {
                    double wallDist = _wallDistances[worldX];
                    double distance = Math.Sqrt(
                        Math.Pow(_spritePositions[2].X - _player.Position.X, 2) +
                        Math.Pow(_spritePositions[2].Y - _player.Position.Y, 2));

                    if (distance >= wallDist)
                    {
                        // Draw a vertical transparent strip to mask this column
                        RectangleShape mask = new RectangleShape(new Vector2f(1, _resolutionY));
                        mask.Position = new Vector2f(x, 0);
                        mask.FillColor = Color.Transparent;
                        renderTexture.Draw(mask, new RenderStates(BlendMode.None));
                    }
                }
            }
            renderTexture.Display();

            // Draw the clipped sprite to the main target
            Sprite clippedSprite = new Sprite(renderTexture.Texture);
            clippedSprite.Position = new Vector2f(spriteLeft, 0);
            target.Draw(clippedSprite);

            // Clean up
            renderTexture.Dispose();
        }
    }
}
