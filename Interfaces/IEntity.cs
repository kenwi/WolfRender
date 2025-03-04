using SFML.Graphics;
using SFML.System;

namespace WolfRender.Interfaces
{
    public interface IEntity
    {
        Vector2f Position { get; set; }
        float Direction { get; set; }
        Sprite Sprite { get; }
        Texture Texture { get;}
        bool IsAlive { get; set; }
    }
} 