using SFML.System;

namespace WolfRender.Interfaces
{
    public interface IPlayer
    {
        Vector2f Position { get; set; }
        float Fov { get; set; }
        double FovHalf { get; set; }
        double Direction { get; set; }
        float RotationSpeed { get; }
        bool IsShooting { get; set; }
    }
}