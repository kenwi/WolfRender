using System;
using System.Collections.Generic;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class Input : Singleton<Input>
    {
        List<Keyboard.Key> keydown = new List<Keyboard.Key>();
        Vector2i previousMousePosition = Mouse.GetPosition();
        float mouseRotationMultiplier = 0.1f;

        public void Init()
        {
            Game.Instance.Window.SetMouseCursorVisible(false);
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update(float dt)
        {
            var player = Game.Instance.Player;
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                player.Direction -= player.RotationSpeed * dt;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                player.Direction += player.RotationSpeed * dt;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                var movementSpeed = player.MovementSpeed;
                var playerDirection = player.Direction;
                var position = new Vector2f(movementSpeed * MathF.Cos(playerDirection) * dt, movementSpeed * MathF.Sin(playerDirection) * dt);
                player.Position += position;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
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

            if (Keyboard.IsKeyPressed(Keyboard.Key.H))
            {
                if (!keydown.Contains(Keyboard.Key.H))
                {
                    Game.Instance.HelpMenuVisible = !Game.Instance.HelpMenuVisible;
                    keydown.Add(Keyboard.Key.H);
                }
            }
            else
            {
                keydown.Remove(Keyboard.Key.H);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.L))
            {
                if (!keydown.Contains(Keyboard.Key.L))
                {
                    Game.Instance.FramerateLimited = !Game.Instance.FramerateLimited;
                    keydown.Add(Keyboard.Key.L);
                }
            }
            else
            {
                keydown.Remove(Keyboard.Key.L);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.M))
            {
                if (!keydown.Contains(Keyboard.Key.M))
                {
                    Game.Instance.MouseVisible = !Game.Instance.MouseVisible;
                    keydown.Add(Keyboard.Key.M);
                }
            }
            else
            {
                keydown.Remove(Keyboard.Key.M);
            }

            var mousePosition = Mouse.GetPosition();
            var mouseDelta = mousePosition - previousMousePosition;
            if (MathF.Sqrt(mouseDelta.X * mouseDelta.X + mouseDelta.Y * mouseDelta.Y) > 0)
            {
                player.Direction += player.RotationSpeed * mouseRotationMultiplier * mouseDelta.X * dt;
            }

            if (!Game.Instance.MouseVisible)
            {
                if (mousePosition.X + 50 > Game.Instance.Window.Size.X + Game.Instance.Window.Position.X)
                {
                    mousePosition.X -= (int)Game.Instance.Window.Size.X / 2;
                    Mouse.SetPosition(new Vector2i(mousePosition.X, mousePosition.Y));
                }
                if (mousePosition.X - 50 <= Game.Instance.Window.Position.X)
                {
                    mousePosition.X += (int)Game.Instance.Window.Size.X / 2;
                    Mouse.SetPosition(new Vector2i(mousePosition.X, mousePosition.Y));
                }
                if (mousePosition.Y + 50 > Game.Instance.Window.Size.Y + Game.Instance.Window.Position.Y)
                {
                    mousePosition.Y -= (int)Game.Instance.Window.Size.Y / 2;
                    Mouse.SetPosition(new Vector2i(mousePosition.X, mousePosition.Y));
                }
                if (mousePosition.Y - 50 <= Game.Instance.Window.Position.Y)
                {
                    mousePosition.Y += (int)Game.Instance.Window.Size.Y / 2;
                    Mouse.SetPosition(new Vector2i(mousePosition.X, mousePosition.Y));
                }
            }
            previousMousePosition = mousePosition;
        }
    }
}
