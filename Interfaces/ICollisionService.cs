using SFML.System;
using WolfRender.Models;

namespace WolfRender.Interfaces
{
    public interface ICollisionService
    {
        Vector2f GetValidPosition(Vector2f currentPosition, Vector2f targetPosition, EntityType entityType = EntityType.Player);
        
        bool HasLineOfSight(Vector2f start, Vector2f end);
    }
    
    public enum EntityType
    {
        Player,
        Enemy,
        Projectile
    }
} 