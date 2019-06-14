using System;
using SFML.Window;

namespace WolfRender
{
    public class InputHandler : Singleton<InputHandler>
    {
        public void Init()
        {
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update(float dt)
        {
            var playerPosition = Game.Instance.Player.Position;
            var playerDirection = Game.Instance.Player.Direction;
            var fov = Game.Instance.Player.Fov;

            var rotationSpeed = 3.1415f / 180 * 100;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                playerDirection -= rotationSpeed * dt;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                playerDirection += rotationSpeed * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                playerPosition.X += 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y += 1 * (float)Math.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerPosition.X -= 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y -= 1 * (float)Math.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                fov += dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                fov -= dt;
            }
            Game.Instance.Player.Position = playerPosition;
            Game.Instance.Player.Direction = playerDirection;
            Game.Instance.Player.Fov = fov;

            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                Game.Instance.Window.Close();
            
            Game.Instance.Window.DispatchEvents();
        }
    }
}
