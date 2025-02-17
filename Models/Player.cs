using SFML.System;
using System;
using WolfRender.Interfaces;

namespace WolfRender.Models
{
    internal class Player : IPlayer
    {
        public Vector2f Position { get; set; } = new Vector2f(5, 5f);
        public float Fov { get; set; } = (float)Math.PI / 2.0f;
        public double FovHalf { get; set; }
        public float Direction { get; set; }
        public float RotationSpeed { get; } = 3.0f;
        public Player()
        {
            FovHalf = Fov / 2.0;
        }
    }
}
