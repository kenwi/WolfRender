using System;
using System.Collections.Generic;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class Input : Singleton<Input>
    {
        List<Keyboard.Key> keydown = new List<Keyboard.Key>();
        float mouseSpeedMultiplier = 0.001f;

        public void Init()
        {
            Mouse.SetPosition(Game.Instance.WindowCenter);
            Game.Instance.Window.SetMouseCursorVisible(false);
            Game.Instance.Window.SetMouseCursorGrabbed(true);
            Game.Instance.Window.Closed += (s, e) => Game.Instance.Window.Close();
        }

        public void Update(float dt)
        {
            var mouseDelta = Mouse.GetPosition() - Game.Instance.WindowCenter;
            PlayerOptions(dt, Game.Instance.Player);
            PlayerMovement(dt, Game.Instance.Player);
            PlayerRotation(dt, Game.Instance.Player, mouseDelta);
            if (!Game.Instance.IsMouseVisible)
            {
                Mouse.SetPosition(Game.Instance.WindowCenter);
            }
        }

        private void PlayerOptions(float dt, Player player)
        {
            checkKeyAction(Keyboard.Key.Escape, () => Game.Instance.Window.Close());
            checkKeyAction(Keyboard.Key.PageUp, () => player.Fov += dt);
            checkKeyAction(Keyboard.Key.PageDown, () => player.Fov -= dt);

            checkToggleAction(Keyboard.Key.H, () => Game.Instance.IsHelpMenuVisible = !Game.Instance.IsHelpMenuVisible);
            checkToggleAction(Keyboard.Key.L, () => Game.Instance.IsFramerateLimited = !Game.Instance.IsFramerateLimited);
            checkToggleAction(Keyboard.Key.M, () => Game.Instance.IsHelpMenuVisible = !Game.Instance.IsMouseVisible);
        }

        private bool checkToggle(Keyboard.Key key)
        {
            bool value = false;
            if (Keyboard.IsKeyPressed(key))
            {
                if (!keydown.Contains(key))
                {
                    keydown.Add(key);
                    value = true;
                }
            }
            else
            {
                keydown.Remove(key);
                value = false;
            }
            return value;
        }

        private void checkToggleAction(Keyboard.Key key, Action action)
        {
            if (checkToggle(key))
                action();
        }

        private void checkKeyAction(Keyboard.Key key, Action action)
        {
            if(Keyboard.IsKeyPressed(key))
                action();
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

            if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                playerDirection = player.Direction;
                addMovement(dt, player, playerDirection);
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.S) || Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerDirection = player.Direction + MathF.PI;
                addMovement(dt, player, playerDirection);
            }
        }

        private void PlayerRotation(float dt, Player player, Vector2i mouseDelta)
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                player.Direction += player.RotationSpeed * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                player.Direction -= player.RotationSpeed * dt;
            }

            if (Game.Instance.IsMouseVisible)
                return;

            if (mouseDelta.X != 0)
            {
                player.Direction += player.RotationSpeed * mouseDelta.X * mouseSpeedMultiplier;
            }
        }

        private static void addMovement(float dt, Player player, float playerDirection)
        {
            player.Position += new Vector2f(player.MovementSpeed * MathF.Cos(playerDirection) * dt,
                                            player.MovementSpeed * MathF.Sin(playerDirection) * dt);
        }
    }
}
