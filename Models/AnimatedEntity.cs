using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using WolfRender.Interfaces;
using WolfRender.Services;

public class AnimatedEntity : IEntity
{
    private readonly IAnimationService _animationService;
    private float _animationTime = 0;
    private string _currentAnimation = "idle";
    private float _frameRate = 4.0f; // 8 frames per second
    private float _walkSpeed = 1.0f;
    private Vector2f _position;
    private float _direction;
    bool _isAnimating = false;
    private float _idleTimer = 0f;
    private const float IDLE_WANDER_INTERVAL = 5.0f;
    private readonly Random _random = new Random();
    private readonly IMapService _mapService;
    private readonly IPlayerService _playerService;
    private bool _canSeePlayer;
    private bool _previousCanSeePlayer;
    private const float FOV = (float)(Math.PI / 2); // 90 degrees in radians
    private const float FOV_HALF = FOV / 2;
    private const float VIEW_DISTANCE = 8.0f; // How far the entity can see

    private List<Vector2f> _currentPath;
    private int _currentPathIndex;
    private float _rotationSpeed = 4.0f; // Radians per second
    private const float POSITION_THRESHOLD = 0.1f; // How close we need to be to a node to consider it reached

    public int SpriteLeft { get; set; }
    public int SpriteRight { get; set; }
    public Sprite Sprite { get; set; }
    public Texture Texture => null; // Not used for animated entities
    public bool IsAlive { get; set; } = true;
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

    public bool IsAnimating
    {
        get => _isAnimating;
        set
        {
            _isAnimating = value;
        }
    }
        
    public bool IsFollowingPath => _currentPath != null && _currentPathIndex < _currentPath.Count;

    public bool CanSeePlayer => _canSeePlayer;

    public AnimatedEntity(
        IAnimationService animationService,
        string sheetName,
        IMapService mapService,
        IPlayerService playerService)
    {
        SheetName = sheetName;
        _animationService = animationService;
        _mapService = mapService;
        _playerService = playerService;
    }
    
    public void Update(float deltaTime)
    {
        if (!IsAlive)
            return;

        // Update animation time
        _animationTime += deltaTime;

        // Check if player is visible
        CheckPlayerVisibility();

        // Handle wandering behavior when idle
        if (GetCurrentAnimation() == "death")
        {
            if (GetAnimationTime() >= 1.5f)
            {
                IsAlive = false;
            }
            return;
        }

        if (_currentAnimation == "idle" && !IsFollowingPath)
        {
            _idleTimer += deltaTime;

            if (_idleTimer >= IDLE_WANDER_INTERVAL)
            {
                TryWanderToNewLocation();
                _idleTimer = 0f; // Reset timer
            }
        }

        if (IsFollowingPath)
        {
            WalkPath(deltaTime);
        }

        if (_canSeePlayer)
        {
            SetAnimation("attack");
            StopFollowingPath();
        }
    }
    
    private void CheckPlayerVisibility()
    {
        _previousCanSeePlayer = _canSeePlayer;
        var player = _playerService.Player;
        
        // Calculate vector to player
        float dx = player.Position.X - Position.X;
        float dy = player.Position.Y - Position.Y;
        
        // Calculate distance to player
        float distanceToPlayer = (float)Math.Sqrt(dx * dx + dy * dy);
        
        // Calculate angle to player (in world space)
        float angleToPlayer = (float)Math.Atan2(-dy, dx); // Negative dy because Y is inverted
        
        // Normalize angles to [0, 2π)
        while (angleToPlayer < 0) angleToPlayer += 2 * (float)Math.PI;
        while (angleToPlayer >= 2 * (float)Math.PI) angleToPlayer -= 2 * (float)Math.PI;
        
        // If player is beyond view distance, they can't be seen
        if (distanceToPlayer > VIEW_DISTANCE)
        {
            _canSeePlayer = false;
        }
        else
        {            
            // Calculate relative angle between entity's direction and player
            float relativeAngle = angleToPlayer - Direction;
            
            // Normalize to [-π, π]
            while (relativeAngle > Math.PI) relativeAngle -= 2 * (float)Math.PI;
            while (relativeAngle < -Math.PI) relativeAngle += 2 * (float)Math.PI;
            
            // Check if player is within FOV cone
            if (Math.Abs(relativeAngle) > FOV_HALF)
            {
                _canSeePlayer = false;
            }
            else
            {
                _canSeePlayer = PerformDDA(player.Position);
                
                // If we can see the player, face them
                if (_canSeePlayer)
                {
                    Direction = angleToPlayer;
                }
            }
        }

        // Log visibility state changes
        if (_canSeePlayer && !_previousCanSeePlayer)
        {
            Console.WriteLine("Spotted the player!");
        }
        else if (!_canSeePlayer && _previousCanSeePlayer)
        {
            Console.WriteLine("Lost sight of the player");

            Vector2i currentPos = new Vector2i((int)Position.X, (int)Position.Y);
            Vector2i targetPos = new Vector2i((int)player.Position.X, (int)player.Position.Y);

            Console.WriteLine($"Wandering to last known player position {targetPos}");
            var path = _mapService.PathFind(currentPos, targetPos);
            WalkPath(path, 0); // Initialize walking the path
        }
    }

    private void TryWanderToNewLocation()
    {
        // Get current position as integers
        Vector2i currentPos = new Vector2i((int)Position.X, (int)Position.Y);
        
        // Try up to 10 times to find a valid path
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Generate random target position within ±10 squares
            Vector2i targetPos = new Vector2i(
                currentPos.X + _random.Next(-10, 10),
                currentPos.Y + _random.Next(-10, 10)
            );

            // Try to find path to target
            var path = _mapService.PathFind(currentPos, targetPos);
            
            if (path != null)
            {
                WalkPath(path, 0); // Initialize walking the path
                return; // Successfully found path
            }
        }
        
        // If we couldn't find a valid path after 10 attempts, reset the timer to try again later
        _idleTimer = IDLE_WANDER_INTERVAL - 1.0f;
    }
    
    public void Walk(float dt)
    {
        SetAnimation("walk");
        _position += new Vector2f(
            _walkSpeed * (float)Math.Cos(_direction),
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
            Console.WriteLine($"Following path of length {path.Count}");

            if(path.Count > 50)
            {
                Console.WriteLine("Path too long");
                StopFollowingPath();
                return;
            }
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
                    StopFollowingPath();
                    SetAnimation("idle");
                }
            }
        }
    }

    public void StopFollowingPath()
    {
        _currentPath = null;
    }

    private bool PerformDDA(Vector2f targetPos)
    {
        // Calculate ray direction from entity to target
        double rayDirX = targetPos.X - Position.X;
        double rayDirY = targetPos.Y - Position.Y;
        
        // Normalize the direction vector
        double length = Math.Sqrt(rayDirX * rayDirX + rayDirY * rayDirY);
        rayDirX /= length;
        rayDirY /= length;

        // Current map position (integer)
        int mapX = (int)Position.X;
        int mapY = (int)Position.Y;

        // Length of ray from one x or y-side to next x or y-side
        double deltaDistX = Math.Abs(1 / rayDirX);
        double deltaDistY = Math.Abs(1 / rayDirY);

        // Calculate step and initial sideDist
        int stepX = rayDirX < 0 ? -1 : 1;
        int stepY = rayDirY < 0 ? -1 : 1;

        // Calculate distance to first x and y intersections
        double sideDistX = rayDirX < 0
            ? (Position.X - mapX) * deltaDistX
            : (mapX + 1.0 - Position.X) * deltaDistX;

        double sideDistY = rayDirY < 0
            ? (Position.Y - mapY) * deltaDistY
            : (mapY + 1.0 - Position.Y) * deltaDistY;

        // Target map position (integer)
        int targetMapX = (int)targetPos.X;
        int targetMapY = (int)targetPos.Y;

        // Perform DDA
        bool hit = false;
        while (!hit)
        {
            // Jump to next map square
            if (sideDistX < sideDistY)
            {
                sideDistX += deltaDistX;
                mapX += stepX;
            }
            else
            {
                sideDistY += deltaDistY;
                mapY += stepY;
            }

            // Check if we've reached the target position
            if (mapX == targetMapX && mapY == targetMapY)
                return true;

            // Check if we hit a wall
            int tileType = _mapService.Get(new Vector2i(mapX, mapY));
            if (tileType > 0 && tileType != 3)
            {
                hit = true;
            }
        }

        return false; // We hit a wall before reaching the target
    }
} 