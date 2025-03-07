using Microsoft.Extensions.Logging;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;

namespace WolfRender.Services
{
    public class AnimationService : IAnimationService
    {
        private readonly ILogger<AnimationService> _logger;
        private readonly ITextureService _textureService;
        private Dictionary<string, Texture> _spriteSheets;
        private Dictionary<string, Dictionary<string, List<Sprite>>> _animations;
        private Dictionary<string, Dictionary<string, List<List<Sprite>>>> _multiRowAnimations;
        private int currentFrameIndex = 0;
        
        // Constants for the sprite sheet
        private const int SPRITE_SIZE = 64;
        private const int SPRITE_PADDING = 1;
        private const int ANGLES_COUNT = 8;  // 8 viewing angles

        public int CurrentFrameIndex => currentFrameIndex;
        
        public AnimationService(ILogger<AnimationService> logger, ITextureService textureService)
        {
            _logger = logger;
            _textureService = textureService;
            _spriteSheets = new Dictionary<string, Texture>();
            _animations = new Dictionary<string, Dictionary<string, List<Sprite>>>();
            _multiRowAnimations = new Dictionary<string, Dictionary<string, List<List<Sprite>>>>();
        }
        
        public void LoadSpriteSheet(string name, string path)
        {
            _textureService.LoadTexture(name, path);
            var image = _textureService.GetTextureImage(name);
            
            // Make magenta background transparent
            image.CreateMaskFromColor(new Color(152, 0, 136));
            
            _spriteSheets[name] = new Texture(image);
            _animations[name] = new Dictionary<string, List<Sprite>>();
            
            _logger.LogInformation($"Loaded sprite sheet: {name}");
        }
        
        public void CreateAnimation(string sheetName, string animationName, int row, int framesCount)
        {
            if (!_spriteSheets.ContainsKey(sheetName))
            {
                _logger.LogError($"Sprite sheet not found: {sheetName}");
                return;
            }
            
            var sprites = new List<Sprite>();
            
            // Extract each frame for each angle
            for (int angle = 0; angle < ANGLES_COUNT; angle++)
            {
                int col = angle;
                
                // Calculate position in the sprite sheet
                int x = col * (SPRITE_SIZE + SPRITE_PADDING);
                int y = row * (SPRITE_SIZE + SPRITE_PADDING);
                
                // Create sprite from the region
                Sprite sprite = new Sprite(_spriteSheets[sheetName], new IntRect(x, y, SPRITE_SIZE, SPRITE_SIZE));
                sprite.Origin = new Vector2f(SPRITE_SIZE / 2, SPRITE_SIZE / 2);
                
                sprites.Add(sprite);
            }
            
            _animations[sheetName][animationName] = sprites;
            _logger.LogInformation($"Created animation: {animationName} from {sheetName}, row {row}");
        }
        
        public void CreateMultiRowAnimation(string sheetName, string animationName, int startRow, int rowCount, int framesPerRow)
        {
            if (!_spriteSheets.ContainsKey(sheetName))
            {
                _logger.LogError($"Sprite sheet not found: {sheetName}");
                return;
            }
            
            var sprites = new List<List<Sprite>>();
            
            if (rowCount == 1)
            {
                var rowSprites = new List<Sprite>();
                for (int column = 0; column < framesPerRow; column++)
                {
                    int x = column * (SPRITE_SIZE + SPRITE_PADDING);
                    int y = startRow * (SPRITE_SIZE + SPRITE_PADDING);
                    Sprite sprite = new Sprite(_spriteSheets[sheetName], new IntRect(x, y, SPRITE_SIZE, SPRITE_SIZE));
                    sprite.Origin = new Vector2f(SPRITE_SIZE / 2, SPRITE_SIZE / 2);
                    rowSprites.Add(sprite);
                }
                sprites.Add(rowSprites);
            }
            else
            {
                // For each row in the animation
                for (int row = 0; row < rowCount; row++)
                {
                    var rowSprites = new List<Sprite>();

                    // For each angle in the row
                    for (int angle = 0; angle < ANGLES_COUNT; angle++)
                    {
                        // Calculate position in the sprite sheet
                        int x = angle * (SPRITE_SIZE + SPRITE_PADDING);
                        int y = (startRow + row) * (SPRITE_SIZE + SPRITE_PADDING);

                        // Create sprite from the region
                        Sprite sprite = new Sprite(_spriteSheets[sheetName], new IntRect(x, y, SPRITE_SIZE, SPRITE_SIZE));
                        sprite.Origin = new Vector2f(SPRITE_SIZE / 2, SPRITE_SIZE / 2);

                        rowSprites.Add(sprite);
                    }

                    sprites.Add(rowSprites);
                }
            }
            
            // Store the multi-row animation
            if (!_multiRowAnimations.ContainsKey(sheetName))
            {
                _multiRowAnimations[sheetName] = new Dictionary<string, List<List<Sprite>>>();
            }
            
            _multiRowAnimations[sheetName][animationName] = sprites;
            _logger.LogInformation($"Created multi-row animation: {animationName} from {sheetName}, rows {startRow}-{startRow+rowCount-1}");
        }
        
        public Sprite GetSprite(string sheetName, string animationName, int angleIndex)
        {
            if (!_animations.ContainsKey(sheetName) || 
                !_animations[sheetName].ContainsKey(animationName))
            {
                _logger.LogError($"Animation not found: {sheetName}/{animationName}");
                return null;
            }
            
            var sprites = _animations[sheetName][animationName];
            angleIndex = Math.Clamp(angleIndex, 0, sprites.Count - 1);
            
            return sprites[angleIndex];
        }
        
        public Sprite GetSprite(string sheetName, string animationName, float angle)
        {
            // Normalize angle to [0, 2π)
            while (angle < 0) angle += MathF.PI * 2;
            while (angle >= MathF.PI * 2) angle -= MathF.PI * 2;
            
            // Convert angle to sprite index (8 directions)
            // 0 = front, 1 = front-right, 2 = right, etc.
            int angleIndex = (int)Math.Round(angle / (MathF.PI * 2) * ANGLES_COUNT) % ANGLES_COUNT;
            
            return GetSprite(sheetName, animationName, angleIndex);
        }
        
        public Sprite GetSpriteForRelativeAngle(string sheetName, string animationName, float relativeAngle)
        {
            // Normalize relative angle to [-π, π]
            while (relativeAngle < -MathF.PI) relativeAngle += MathF.PI * 2;
            while (relativeAngle > MathF.PI) relativeAngle -= MathF.PI * 2;
            
            // Map the relative angle to sprite index
            // 0 = front (relative angle = 0)
            // 1 = front-right (relative angle = -π/4)
            // 2 = right (relative angle = -π/2)
            // etc.
            
            // Convert to [0, 2π) for easier mapping
            float normalizedAngle = relativeAngle + MathF.PI;
            int angleIndex = (int)Math.Round(normalizedAngle / (MathF.PI * 2) * ANGLES_COUNT) % ANGLES_COUNT;
            
            return GetSprite(sheetName, animationName, angleIndex);
        }
        
        public Sprite GetAnimationFrame(string sheetName, string animationName, int angleIndex, float animationTime, float frameRate)
        {
            if (!_multiRowAnimations.ContainsKey(sheetName) || 
                !_multiRowAnimations[sheetName].ContainsKey(animationName))
            {
                _logger.LogError($"Multi-row animation not found: {sheetName}/{animationName}");
                return null;
            }
            
            var animation = _multiRowAnimations[sheetName][animationName];
            int totalFrames = animation.Count;
            
            // Calculate current frame based on time and frame rate
            int frameIndex = (int)(animationTime * frameRate) % totalFrames;
            
            // Get the sprites for the current frame
            var frameSprites = animation[frameIndex];
            
            // Clamp angle index to valid range
            angleIndex = Math.Clamp(angleIndex, 0, frameSprites.Count - 1);

            return frameSprites[angleIndex];
        }

        public Sprite GetAnimationFrame(string sheetName, string animationName, float animationTime, float frameRate)
        {
            if (!_multiRowAnimations.ContainsKey(sheetName) ||
                !_multiRowAnimations[sheetName].ContainsKey(animationName))
            {
                _logger.LogError($"Multi-row animation not found: {sheetName}/{animationName}");
                return null;
            }

            var animation = _multiRowAnimations[sheetName][animationName];
            int totalFrames = animation[0].Count;

            // Calculate current frame based on time and frame rate
            int frameIndex = (int)(animationTime * frameRate * 0.5) % totalFrames;
            currentFrameIndex = frameIndex;

            // Get the sprites for the current frame
            var frameSprites = animation[0];
            
            return frameSprites[frameIndex];
        }
    }
} 