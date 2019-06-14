using SFML.System;

namespace WolfRender
{
    public class Player
    {
        public Vector2f Position { get; set; }
        public float Direction { get; set; }
        public float Fov { get; set; }
        public float MovementSpeed { get; set; }
        public float RotationSpeed { get; set; }
    }
}
