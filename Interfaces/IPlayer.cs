using SFML.System;

namespace WolfRender.Interfaces
{
    internal interface IPlayer
    {
        Vector2f Position { get; set; }
        float Fov { get; set; }
        double FovHalf { get; set; }
        double Direction { get; set; }
        float RotationSpeed { get; }
    }
}