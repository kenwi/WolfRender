using SFML.Graphics;
using SFML.System;
using WolfRender.Interfaces;

public class AnimatedEntity : IEntity
{
    private readonly IAnimationService _animationService;
    private float _animationTime = 0;
    private string _currentAnimation = "idle";
    private float _frameRate = 4.0f; // 8 frames per second
    
    public Vector2f Position { get; set; }
    public float Direction { get; set; }
    public Sprite Sprite { get; private set; }
    public Texture Texture => null; // Not used for animated entities
    
    public AnimatedEntity(IAnimationService animationService, string sheetName)
    {
        _animationService = animationService;
        SheetName = sheetName;
    }
    
    public string SheetName { get; }
    
    public void Update(float deltaTime)
    {
        // Update animation time
        _animationTime += deltaTime;
        
        // Calculate sprite index based on player position
        // This will be done in the renderer
    }
    
    public void SetAnimation(string animationName)
    {
        if (_currentAnimation != animationName)
        {
            _currentAnimation = animationName;
            _animationTime = 0; // Reset animation time when changing animations
        }
    }
    
    public string GetCurrentAnimation()
    {
        return _currentAnimation;
    }
    
    public float GetAnimationTime()
    {
        return _animationTime;
    }
    
    public float GetFrameRate()
    {
        return _frameRate;
    }
} 