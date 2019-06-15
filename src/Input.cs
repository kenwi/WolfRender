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
            Game.Instance.Window.SetMouseCursorGrabbed(true);
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update(float dt)
        {
            var player = Game.Instance.Player;
            PlayerOptions(dt, player);
            PlayerMovement(dt, player);
            PlayerRotation(dt, player);
        }
        
        private static void addMovement(float dt, Player player, float playerDirection)
        {
            player.Position += new Vector2f(player.MovementSpeed * MathF.Cos(playerDirection) * dt, 
                                            player.MovementSpeed * MathF.Sin(playerDirection) * dt);
        }

        private void PlayerOptions(float dt, Player player)
        {
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
        }

        private static void PlayerMovement(float dt, Player player)
        {
            float playerDirection = player.Direction;
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                playerDirection = player.Direction - MathF.PI * 0.5f;
                addMovement(dt, player, playerDirection);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                playerDirection = player.Direction + MathF.PI * 0.5f;
                addMovement(dt, player, playerDirection);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.W))
            {
                playerDirection = player.Direction;
                addMovement(dt, player, playerDirection);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                playerDirection = player.Direction + MathF.PI;
                addMovement(dt, player, playerDirection);
            }
        }

        private void PlayerRotation(float dt, Player player)
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                player.Direction += player.RotationSpeed * mouseRotationMultiplier * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                player.Direction -= player.RotationSpeed * mouseRotationMultiplier * dt;
            }

            var mousePosition = Mouse.GetPosition();
            var mouseDelta = mousePosition - previousMousePosition;
            if (mouseDelta.X != 0)
            {
                player.Direction += player.RotationSpeed * mouseRotationMultiplier * mouseDelta.X * dt;
            }

            if (!Game.Instance.MouseVisible)
            {
                if (mousePosition.X <= Game.Instance.Window.Position.X + 10 || mousePosition.X >= Game.Instance.Window.Position.X + Game.Instance.Window.Size.X)
                {
                    var center = new Vector2i((int)VideoMode.DesktopMode.Width / 2, (int)VideoMode.DesktopMode.Height / 2);
                    previousMousePosition = center;
                    Mouse.SetPosition(center);
                    return;
                }
            }
            previousMousePosition = mousePosition;
        }
    }
}
