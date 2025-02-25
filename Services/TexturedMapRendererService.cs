using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WolfRender.Interfaces;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class TexturedMapRendererService : Drawable, IMapRendererService
    {
        private readonly ILogger<TexturedMapRendererService> _logger;
        private readonly ITextureService _textureService;
        private readonly WindowConfiguration _windowConfiguration;
        private readonly GameConfiguration _gameConfiguration;
        private readonly IPlayerService _playerService;
        private readonly IMapService _mapService;
        
        private IPlayer _player;
        private int _textureSize;
        private int[] _bluestonePixels;
        private int[] _mossyPixels;
        private Texture _texture;
        private Sprite _sprite;
        private byte[] _bytes;
        private int[] _pixels;
        private float[] _zBuffer;
        private int _halfHeight;
        private int _resolutionY;
        private int _resolutionX;
        private List<int[]> _textures;
        private Texture _minimapTexture;
        private Texture _barrelTexture;
        private Sprite _barrelSprite;
        private List<Vector2f> _spritePositions;
        private double[] _wallDistances;
        private Texture _debugTexture;
        private Sprite _debugSprite;
        private byte[] _debugBytes;
        
        public Texture MapTexture => _minimapTexture;

        public TexturedMapRendererService(
            ILogger<TexturedMapRendererService> logger,
            ITextureService textureService,
            IOptions<WindowConfiguration> windowConfiguration,
            IOptions<GameConfiguration> gameConfiguration,
            IPlayerService playerService,
            IMapService mapService)
        {
            _logger = logger;
            _textureService = textureService;
            _windowConfiguration = windowConfiguration.Value;
            _gameConfiguration = gameConfiguration.Value;
            _playerService = playerService;
            _mapService = mapService;
            _logger.LogInformation("TexturedMapRendererService starting");
        }

        public void Init()
        {
            _textures = new List<int[]>();
            _textureService.LoadTexture("bluestone", "Assets/bluestone.png");
            _textureService.LoadTexture("greystone", "Assets/greystone.png");
            _textureService.LoadTexture("redbrick", "Assets/redbrick.png");
            _textureService.LoadTexture("wood", "Assets/wood.png");
            _bluestonePixels = _textureService.GetTextureArray("greystone");

            _textures.Add(_textureService.GetTextureArray("greystone"));
            _textures.Add(_textureService.GetTextureArray("greystone"));
            _textures.Add(_textureService.GetTextureArray("redbrick"));
            _textures.Add(_textureService.GetTextureArray("wood")); // Wood floor
            _textures.Add(_textureService.GetTextureArray("wood")); // Wood wall
            _textures.Add(_textureService.GetTextureArray("bluestone")); // Bluestone wall


            var minimapImage = _textureService.GetTextureImage("level1");
            _minimapTexture = new Texture(minimapImage);

            _textureService.LoadTexture("mossy", "Assets/mossy.png");
            _mossyPixels = _textureService.GetTextureArray("mossy");

            // Create a new sprite with transparency
            _textureService.LoadTexture("barrel", "Assets/barrel.png");
            var barrelImage = _textureService.GetTextureImage("barrel");
            barrelImage.CreateMaskFromColor(Color.Black);
            _barrelTexture = new Texture(barrelImage);
            _barrelSprite = new Sprite(_barrelTexture);
            _barrelSprite.Position = new Vector2f(20.0f, 45.5f);
            _barrelSprite.Origin = new Vector2f(_barrelTexture.Size.X / 2, _barrelTexture.Size.Y / 2);
            
            _resolutionX = _gameConfiguration.Resolution.X;
            _resolutionY = _gameConfiguration.Resolution.Y;

            _texture = new Texture((uint)_resolutionX, (uint)_resolutionY);
            _sprite = new Sprite(_texture);
            _bytes = new byte[_resolutionX * _resolutionY * 4];
            _pixels = new int[_resolutionX * _resolutionY];
            _zBuffer = new float[_resolutionY];  
            _player = _playerService.Player;
            _textureSize = (int)Math.Sqrt(_bluestonePixels.Length);
            _halfHeight = _resolutionY / 2;

            _spritePositions = new List<Vector2f>
            {
                new Vector2f(20.5f, 46.5f),
                new Vector2f(20.5f, 44.5f),
            };

            _wallDistances = new double[_resolutionX];

            CalculateZBuffer();

            // Initialize debug visualization
            _debugTexture = new Texture((uint)_resolutionX, (uint)_resolutionY);
            _debugSprite = new Sprite(_debugTexture);
            _debugBytes = new byte[_resolutionX * _resolutionY * 4];
            

            _logger.LogInformation("TexturedMapRendererService initialized");
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            Parallel.For(0, (int)_gameConfiguration.Resolution.X, x =>
            {
                double angle = _player.Direction - _player.FovHalf + (x * _player.Fov) / _gameConfiguration.Resolution.X;

                // Precalculate ray direction
                double rayDirX = Math.Cos(angle);
                double rayDirY = Math.Sin(angle);

                // Current map position
                int mapX = (int)_player.Position.X;
                int mapY = (int)_player.Position.Y;

                // Length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Abs(1 / rayDirX) * 1;
                double deltaDistY = Math.Abs(1 / rayDirY) * 1;

                // Calculate step and initial sideDist
                int stepX = rayDirX < 0 ? -1 : 1;
                int stepY = rayDirY < 0 ? -1 : 1;

                double sideDistX = rayDirX < 0
                    ? (_player.Position.X - mapX) * deltaDistX
                    : (mapX + 1.0 - _player.Position.X) * deltaDistX;

                double sideDistY = rayDirY < 0
                    ? (_player.Position.Y - mapY) * deltaDistY
                    : (mapY + 1.0 - _player.Position.Y) * deltaDistY;

                // Perform DDA
                bool hit = false;
                int side = 0;
                int textureIdx = 0;
                while (!hit)
                {
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    textureIdx = _mapService.Get(new Vector2i(mapX, mapY));
                    if (textureIdx > 0)
                        hit = true;

                    if (textureIdx == 3)
                        hit = false;
                }

                // Calculate perpendicular distance
                double perpWallDist = side == 0
                    ? (mapX - _player.Position.X + (1 - stepX) / 2) / rayDirX
                    : (mapY - _player.Position.Y + (1 - stepY) / 2) / rayDirY;

                // Store the actual wall distance
                _wallDistances[x] = perpWallDist;

                // Calculate wall height and drawing bounds
                int lineHeight = (int)(_resolutionY / perpWallDist);
                int drawStart = Math.Max(0, -lineHeight / 2 + _halfHeight);
                int drawEnd = Math.Min(_resolutionY - 1, lineHeight / 2 + _halfHeight);

                // Calculate wall texture coordinates
                double wallX = side == 0
                    ? _player.Position.Y + perpWallDist * rayDirY
                    : _player.Position.X + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                // Calculate texture coordinates based on the 64x64 texture size
                int wallTexX = (int)(wallX * _textureSize) & (_textureSize - 1); // Use 64 for texture size
                if ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0))
                    wallTexX = (_textureSize - 1) - wallTexX;

                // Precalculate texture step and position
                double step = _textureSize / (double)lineHeight; // Use 64 for texture size
                double texPos = (drawStart - _halfHeight + lineHeight / 2.0) * step;

                // Calculate wall shading using perpendicular distance
                float wallShade = CalculateShade(perpWallDist);

                // Draw walls with BlueStone texture
                int xOffset = x;
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int wallTexY = (int)texPos & (_textureSize - 1); // Use 64 for texture size
                    texPos += step;

                    // Get the color from the BlueStone texture
                    int colorIdx = wallTexX + wallTexY * _textureSize; // Corrected index calculation
                    int[] currentTexture = _textures[textureIdx];
                    int wallColor = currentTexture[colorIdx];

                    byte r = (byte)((wallColor & 0xFF) * wallShade);
                    byte g = (byte)((wallColor >> 8 & 0xFF) * wallShade);
                    byte b = (byte)((wallColor >> 16 & 0xFF) * wallShade);
                    _pixels[xOffset + y * _resolutionX] = Tools.PackColor(r, g, b);
                }

                // Draw floor and ceiling with distance-based shading
                for (int y = 0; y < drawStart; y++)
                {
                    // Lookup distance shade to the floor/ceiling from zBuffer
                    float ceilingShade = _zBuffer[y];
                    float floorShade = ceilingShade;

                    // Ceiling
                    int ceilingY = _resolutionY - y - 1;
                    var (ceilingTexX, ceilingTexY, ceilingTextureIdx) = GetFloorTexCoord64x64(x, ceilingY, angle);

                    // Get the color from the GreyStone texture
                    int ceilingColorIdx = ceilingTexX + ceilingTexY * _textureSize;
                    int ceilingColor = _mossyPixels[ceilingColorIdx];

                    // Apply ceiling shading
                    byte r = (byte)((ceilingColor & 0xFF) * ceilingShade);
                    byte g = (byte)((ceilingColor >> 8 & 0xFF) * ceilingShade);
                    byte b = (byte)((ceilingColor >> 16 & 0xFF) * ceilingShade);
                    _pixels[xOffset + y * _resolutionX] = Tools.PackColor(r, g, b);

                    // Floor
                    int floorY = _resolutionY - y - 1;
                    var (floorTexX, floorTexY, floorTextureIdx) = GetFloorTexCoord64x64(x, floorY, angle);

                    // Get the color from the GreyStone texture
                    int floorColorIdx = floorTexX + floorTexY * _textureSize;
                    int[] currentTexture = _textures[floorTextureIdx];
                    int floorColor = currentTexture[floorColorIdx];

                    // Apply floor shading
                    r = (byte)((floorColor & 0xFF) * floorShade);
                    g = (byte)((floorColor >> 8 & 0xFF) * floorShade);
                    b = (byte)((floorColor >> 16 & 0xFF) * floorShade);
                    _pixels[xOffset + floorY * _resolutionX] = Tools.PackColor(r, g, b);
                }
            });
            UpdateScreenTexture(target, states);
        }

        private void UpdateScreenTexture(RenderTarget target, RenderStates states)
        {
            Buffer.BlockCopy(_pixels, 0, _bytes, 0, _bytes.Length);
            _texture.Update(_bytes);
            target.Draw(_sprite, states);
            RenderSprites(target, states);
            //UpdateDebugVisualization(target);
        }

        private void RenderSprites(RenderTarget target, RenderStates states)
        {
            foreach (var spritePosition in _spritePositions)
            {
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
        }

        public void DebugSpritesDistance()
        {
            foreach (var spritePosition in _spritePositions)
            {
                // Calculate vector from player to sprite
                double spriteX = spritePosition.X - _player.Position.X;
                double spriteY = spritePosition.Y - _player.Position.Y;
                
                // Calculate distance
                double distance = Math.Sqrt(spriteX * spriteX + spriteY * spriteY);
                
                // Calculate angle between player's direction and sprite
                double playerToSpriteAngle = Math.Atan2(spriteY, spriteX);
                double relativeAngle = playerToSpriteAngle - _player.Direction;
                
                // Normalize angle to [-PI, PI]
                while (relativeAngle > Math.PI) relativeAngle -= 2 * Math.PI;
                while (relativeAngle < -Math.PI) relativeAngle += 2 * Math.PI;

                // Convert angle to degrees for readability
                double angleInDegrees = relativeAngle * 180 / Math.PI;

                _logger.LogInformation(
                    "Sprite at ({X:F1}, {Y:F1}): Distance = {Distance:F2}, Angle = {Angle:F1}°",
                    spritePosition.X, spritePosition.Y, distance, angleInDegrees);
            }
        }

        private float CalculateShade(double distance)
        {
            const float maxDistance = 64.0f;  // Increased for better floor visibility
            const float minShade = 0.1f;     // Darker minimum
            
            // Add exponential falloff for more dramatic distance shading
            float shade = (float)Math.Pow(1.0f - (distance / maxDistance), _gameConfiguration.ShadingExponent);
            return Math.Max(minShade, shade);
        }

        (int, int, int) GetFloorTexCoord64x64(int x, int y, double angle)
        {
            // Use the absolute angle directly - no need to adjust for player direction since it's already included
            double rayDirX = Math.Cos(angle);
            double rayDirY = Math.Sin(angle);

            // Current y position compared to the center of the screen (the horizon)
            double currentDist = _resolutionY / (2.0 * y - _resolutionY);

            // Calculate the real world coordinates of the point on the floor
            double floorX = _player.Position.X + currentDist * rayDirX;
            double floorY = _player.Position.Y + currentDist * rayDirY;

            int textureId = _mapService.Get(new Vector2i((int)floorX, (int)floorY));

            // Get the texture coordinates
            int texX = (int)(floorX * _textureSize) % _textureSize;
            int texY = (int)(floorY * _textureSize) % _textureSize;

            if (texX < 0) texX += _textureSize;
            if (texY < 0) texY += _textureSize;

            return (texX, texY, textureId);
        }

        public void CalculateZBuffer()
        {
            for (int y = 0; y < _resolutionY; y++)
            {
                double currentDist = Math.Abs(_resolutionY / (2.0 * y - _resolutionY)); // Ensure it's positive
                float ceilingShade = CalculateShade(currentDist);
                _zBuffer[y] = ceilingShade;
            }
        }

        private void UpdateDebugVisualization(RenderTarget target)
        {
            // Clear the debug bytes
            Array.Clear(_debugBytes, 0, _debugBytes.Length);

            // For each column
            for (int x = 0; x < _resolutionX; x++)
            {
                // Convert wall distance to a color intensity
                double distance = _wallDistances[x];
                byte intensity = (byte)(Math.Min(255, (distance * 10))); // Adjust multiplier to taste
                
                // Draw a vertical line in this column
                for (int y = 0; y < _resolutionY; y++)
                {
                    int pixelIndex = (y * _resolutionX + x) * 4;
                    _debugBytes[pixelIndex] = intensity;     // R
                    _debugBytes[pixelIndex + 1] = 0;        // G
                    _debugBytes[pixelIndex + 2] = 0;        // B
                    _debugBytes[pixelIndex + 3] = 200;      // A (semi-transparent)
                }
            }

            // Update and draw the debug visualization
            _debugTexture.Update(_debugBytes);
            target.Draw(_debugSprite);
        }
    }
}
