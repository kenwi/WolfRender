using SFML.System;
using WolfRender.Interfaces;
using System;
using Microsoft.Extensions.Logging;

namespace WolfRender.Services
{
    public class CollisionService : ICollisionService
    {
        private readonly IMapService _mapService;
        private readonly ILogger<CollisionService> _logger;
        
        private readonly int[][] _walkablePixelIds = {
            new[] { 0, 3 },  // Player walkable IDs
            new[] { 0 }      // Enemy walkable IDs
        };
        
        public CollisionService(IMapService mapService, ILogger<CollisionService> logger)
        {
            _mapService = mapService;
            _logger = logger;
        }
        
        public Vector2f GetValidPosition(Vector2f currentPosition, Vector2f targetPosition, EntityType entityType = EntityType.Player)
        {
            Vector2f validPosition = currentPosition;
            int[] walkableIds = _walkablePixelIds[(int)entityType];
            
            // Check X-axis movement
            foreach (var id in walkableIds)
            {
                if (_mapService.Get(new Vector2i((int)targetPosition.X, (int)currentPosition.Y)) == id)
                {
                    validPosition.X = targetPosition.X;
                    break;
                }
            }
            
            // Check Y-axis movement
            foreach (var id in walkableIds)
            {
                if (_mapService.Get(new Vector2i((int)currentPosition.X, (int)targetPosition.Y)) == id)
                {
                    validPosition.Y = targetPosition.Y;
                    break;
                }
            }
            
            return validPosition;
        }
        
        public bool HasLineOfSight(Vector2f start, Vector2f end)
        {
            // DDA algorithm for line of sight checking
            float x = start.X;
            float y = start.Y;
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            
            // Early return if points are the same
            if (distance < 0.001f) return true;
            
            // Normalize direction vector
            dx /= distance;
            dy /= distance;
            
            // Current map position (integer)
            int mapX = (int)start.X;
            int mapY = (int)start.Y;
            
            // Target map position
            int targetMapX = (int)end.X;
            int targetMapY = (int)end.Y;
            
            // Length of ray from one x or y-side to next x or y-side
            double deltaDistX = Math.Abs(1 / dx);
            double deltaDistY = Math.Abs(1 / dy);
            
            // Calculate step and initial sideDist
            int stepX = dx < 0 ? -1 : 1;
            int stepY = dy < 0 ? -1 : 1;
            
            // Calculate distance to first x and y intersections
            double sideDistX = dx < 0
                ? (start.X - mapX) * deltaDistX
                : (mapX + 1.0 - start.X) * deltaDistX;
                
            double sideDistY = dy < 0
                ? (start.Y - mapY) * deltaDistY
                : (mapY + 1.0 - start.Y) * deltaDistY;
            
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
            
            return false; // Hit a wall before reaching the target
        }
    }
} 