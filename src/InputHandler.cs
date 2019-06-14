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
            var rotationSpeed = Tools.DegToRad(100);

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
                playerPosition.X += MathF.Cos(playerDirection) * dt;
                playerPosition.Y += MathF.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerPosition.X -= MathF.Cos(playerDirection) * dt;
                playerPosition.Y -= MathF.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                fov += dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                fov -= dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
            {
                Game.Instance.Window.Close();
            }
            Game.Instance.Player.Position = playerPosition;
            Game.Instance.Player.Direction = playerDirection;
            Game.Instance.Player.Fov = fov;
            Game.Instance.Window.DispatchEvents();
        }
    }
}
