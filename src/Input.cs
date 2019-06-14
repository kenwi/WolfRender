using System;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class Input : Singleton<Input>
    {
        public void Init()
        {
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update(float dt)
        {
            var player = Game.Instance.Player;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                player.Direction -= player.RotationSpeed * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                player.Direction += player.RotationSpeed * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                var movementSpeed = player.MovementSpeed;
                var playerDirection = player.Direction;
                var position = new Vector2f(movementSpeed * MathF.Cos(playerDirection) * dt, movementSpeed * MathF.Sin(playerDirection) * dt);
                player.Position += position;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                var movementSpeed = player.MovementSpeed;
                var playerDirection = player.Direction;
                var position = new Vector2f(movementSpeed * MathF.Cos(playerDirection) * dt, movementSpeed * MathF.Sin(playerDirection) * dt);
                player.Position -= position;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                player.Fov += dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                player.Fov -= dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
            {
                Game.Instance.Window.Close();
            }
        }
    }
}
