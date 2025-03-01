using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;

public class AnimatedEntity : IEntity
{
    private readonly IAnimationService _animationService;
    private float _animationTime = 0;
    private string _currentAnimation = "idle";
    private float _frameRate = 4.0f; // 8 frames per second
    private float _walkSpeed = 1.0f;
    private Vector2f _position;
    private float _direction;
    
    private List<Vector2f> _currentPath;
    private int _currentPathIndex;
    private float _rotationSpeed = 4.0f; // Radians per second
    private const float POSITION_THRESHOLD = 0.1f; // How close we need to be to a node to consider it reached
    
    public Sprite Sprite { get; private set; }
    public Texture Texture => null; // Not used for animated entities
    public bool IsAlive { get; set; }
    public string SheetName { get; }

    public Vector2f Position
    {
        get => _position;
        set
        {
            _position = value;
        }
    }

    public float Direction
    {
        get => _direction;
        set
        {
            _direction = value;
        }
    }
        
    public bool IsFollowingPath => _currentPath != null && _currentPathIndex < _currentPath.Count;

    public AnimatedEntity(IAnimationService animationService, string sheetName)
    {
        _animationService = animationService;
        SheetName = sheetName;
    }
    
    public void Update(float deltaTime)
    {
        // Update animation time
        _animationTime += deltaTime;
        
        // Calculate sprite index based on player position
        // This will be done in the renderer
    }
    
    public void Walk(float dt)
    {
        SetAnimation("walk");
        _position += new Vector2f(_walkSpeed * (float)Math.Cos(_direction),
                            _walkSpeed * -(float)Math.Sin(_direction)) * dt;
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

    public void WalkPath(float dt)
    {
        WalkPath(_currentPath, dt);
    }

    public void WalkPath(List<Vector2f> path, float dt)
    {
        if (path == null || path.Count == 0)
            return;

        // Initialize path if it's a new one
        if (_currentPath != path)
        {
            _currentPath = path;
            _currentPathIndex = 0;
        }

        // Get current target node
        Vector2f targetPos = _currentPath[_currentPathIndex];
        
        // Calculate direction to target
        float dx = targetPos.X - Position.X;
        float dy = targetPos.Y - Position.Y;
        float targetAngle = (float)Math.Atan2(-dy, dx); // Negative dy because Y is inverted
        
        // Normalize target angle to [0, 2π)
        while (targetAngle < 0) targetAngle += 2 * (float)Math.PI;
        while (targetAngle >= 2 * (float)Math.PI) targetAngle -= 2 * (float)Math.PI;
        
        // Calculate shortest rotation direction
        float angleDiff = targetAngle - Direction;
        while (angleDiff > Math.PI) angleDiff -= 2 * (float)Math.PI;
        while (angleDiff < -Math.PI) angleDiff += 2 * (float)Math.PI;
        
        // Rotate towards target
        if (Math.Abs(angleDiff) > 0.1f) // Small threshold to prevent jittering
        {
            float rotation = Math.Sign(angleDiff) * _rotationSpeed * dt;
            // Don't overshoot
            if (Math.Abs(rotation) > Math.Abs(angleDiff))
                rotation = angleDiff;
                
            Direction += rotation;
            
            // Normalize direction
            while (Direction < 0) Direction += 2 * (float)Math.PI;
            while (Direction >= 2 * (float)Math.PI) Direction -= 2 * (float)Math.PI;
        }
        else // We're facing the right direction, walk forward
        {
            Walk(dt);
            
            // Check if we've reached the current node
            float distanceToTarget = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distanceToTarget < POSITION_THRESHOLD)
            {
                _currentPathIndex++;
                
                // Check if we've reached the end of the path
                if (_currentPathIndex >= _currentPath.Count)
                {
                    _currentPath = null;
                    SetAnimation("idle");
                }
            }
        }
    }

    public void StopFollowingPath()
    {
        _currentPath = null;
        SetAnimation("idle");
    }
} 