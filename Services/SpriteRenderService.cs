using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class SpriteRenderService : Drawable, ISpriteRendererService
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
            _animationService.CreateAnimation("guard", "idle", 0, 8); // First row, 8 angles

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

        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _wallDistances = _mapService.WallDistances;
            for (int i = 0; i < 2; i++)
            {
                var spritePosition = _spritePositions[i];

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
                    _barrelSprite.Scale = new Vector2f(scale * 2, scale * 2);
                    _barrelSprite.Position = new Vector2f(screenX, _resolutionY / 2);
                    target.Draw(_barrelSprite, states);
                }
                else if (isPartiallyVisible)
                {
                    // Create a clipped version of the sprite
                    RenderPartiallyOccludedSprite(target, spriteLeft, spriteRight, screenX, scale, distance, columnVisibility);
                }
                // If not visible at all, don't render
            }

            // Render the guard
            if (_spritePositions.Count > 2)
            {
                var guardPosition = _spritePositions[2]; // Use the correct guard position

                // Calculate vector from player to guard
                double spriteX = guardPosition.X - _player.Position.X;
                double spriteY = guardPosition.Y - _player.Position.Y;

                // Calculate angle between player and guard in world space
                double playerToGuardAngle = Math.Atan2(spriteY, spriteX);

                // Normalize to [0, 2π)
                while (playerToGuardAngle < 0) playerToGuardAngle += 2 * Math.PI;
                while (playerToGuardAngle >= 2 * Math.PI) playerToGuardAngle -= 2 * Math.PI;

                // Calculate sprite index (8 directions)
                // The sprite sheet has 0 = front, 4 = back
                // We need to map the angle so that:
                // - When player is at 0 degrees (in front), we use sprite 0
                // - When player is at 180 degrees (behind), we use sprite 4
                int spriteIndex = (int)Math.Round(playerToGuardAngle / (Math.PI * 2) * 8) % 8;

                // Adjust the index to match the sprite sheet orientation
                // First flip horizontally
                spriteIndex = (8 - spriteIndex) % 8;
                // Then rotate 180 degrees (add 4 and wrap around)
                spriteIndex = (spriteIndex + 4) % 8;

                // Get the appropriate guard sprite based on the calculated index
                Sprite guardSprite = _animationService.GetSprite("guard", "idle", spriteIndex);

                // Calculate relative angle for screen positioning
                double relativeAngle = playerToGuardAngle - _player.Direction;

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
                    guardSprite.Scale = new Vector2f(scale * 2, scale * 2);
                    guardSprite.Position = new Vector2f(screenX, _resolutionY / 2);
                    target.Draw(guardSprite, states);
                }
                else if (isPartiallyVisible)
                {
                    // Create a clipped version of the sprite
                    RenderPartiallyOccludedGuard(target, spriteLeft, spriteRight, screenX, scale, guardSprite);
                }

                // For debugging
                if (Input.IsKeyPressed(Keyboard.Key.Space))
                {
                    Console.WriteLine($"Player: {_player.Position}, Guard: {guardPosition}");
                    Console.WriteLine($"Angle: {playerToGuardAngle * 180 / Math.PI}°, Sprite: {spriteIndex}");
                }
            }

        }

        private void RenderPartiallyOccludedSprite(RenderTarget target, int spriteLeft, int spriteRight,
            float screenX, float scale, double distance, bool[] columnVisibility)
        {
            // Create a render texture the size of the sprite on screen
            int spriteWidth = spriteRight - spriteLeft + 1;
            RenderTexture renderTexture = new RenderTexture((uint)spriteWidth, (uint)_resolutionY);
            renderTexture.Clear(Color.Transparent);

            // Draw the original sprite to the render texture
            Sprite tempSprite = new Sprite(_barrelTexture);
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
