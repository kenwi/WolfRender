using SFML.System;
using System;

namespace WolfRender
{
    public class Player
    {
        private Vector2f position;
        public Vector2f Position
        {
            get => position;
            set => position = value;
        }
        public float Direction { get; set; }
        public float Fov { get; set; }
        public float MovementSpeed { get; set; }
        public float RotationSpeed { get; } = 3.0f;
        
        // Movement properties
        private Vector2f velocity;
        private Vector2f acceleration;
        private readonly float maxSpeed = 16.0f;
        private readonly float accelerationRate = 25.0f;
        private readonly float friction = 8.0f;
        private readonly float rotationSpeed = 3.0f;
        
        public Player(Vector2f position, float direction)
        {
            this.position = position;
            Direction = direction;
            velocity = new Vector2f(0, 0);
            acceleration = new Vector2f(0, 0);
        }

        public void Update(float deltaTime, Map map)
        {
            // Get input direction
            Vector2f input = new Vector2f(0, 0);
            
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.W))
                input.X += 1;
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.S))
                input.X -= 1;
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.D))
                input.Y += 1;
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.A))
                input.Y -= 1;

            // Calculate movement direction based on player's facing direction
            float cos = (float)Math.Cos(Direction);
            float sin = (float)Math.Sin(Direction);
            
            // Transform input to world space
            acceleration = new Vector2f(
                (input.X * cos - input.Y * sin) * accelerationRate,
                (input.X * sin + input.Y * cos) * accelerationRate
            );

            // Apply acceleration
            velocity.X += acceleration.X * deltaTime;
            velocity.Y += acceleration.Y * deltaTime;

            // Apply friction
            velocity.X -= velocity.X * friction * deltaTime;
            velocity.Y -= velocity.Y * friction * deltaTime;

            // Clamp velocity to maximum speed
            float speed = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);
            if (speed > maxSpeed)
            {
                velocity.X = (velocity.X / speed) * maxSpeed;
                velocity.Y = (velocity.Y / speed) * maxSpeed;
            }

            // Calculate new position
            Vector2f newPosition = new Vector2f(
                position.X + velocity.X * deltaTime,
                position.Y + velocity.Y * deltaTime
            );

            // Collision detection with walls
            if (map.Get(new Vector2i((int)newPosition.X, (int)position.Y)) == 0)
                position.X = newPosition.X;
            if (map.Get(new Vector2i((int)position.X, (int)newPosition.Y)) == 0)
                position.Y = newPosition.Y;

            // Handle rotation
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Left))
                Direction -= rotationSpeed * deltaTime;
            if (Input.IsKeyPressed(SFML.Window.Keyboard.Key.Right))
                Direction += rotationSpeed * deltaTime;
        }
    }
}
