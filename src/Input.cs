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
            Game.Instance.Window.MouseWheelScrolled += (s, e) =>
            {
                Game.Instance.MapRenderer.Fov += e.Delta * 0.1f;
                Game.Instance.MapRenderer.FovHalf = Game.Instance.MapRenderer.Fov * 0.5f;
                Game.Instance.MapRenderer.UpdateFovCone();
            };
        }

        public void Update(float dt)
        {
            var mouseDelta = Mouse.GetPosition() - Game.Instance.WindowCenter;
            PlayerOptions(dt, Game.Instance.Player);
            Game.Instance.Player.Update(dt, Game.Instance.MapRenderer.Map);
            PlayerRotation(dt, Game.Instance.Player, mouseDelta);
            if (!Game.Instance.MouseVisible)
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
            checkToggleAction(Keyboard.Key.M, () => Game.Instance.MouseVisible = !Game.Instance.MouseVisible);
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

        private void PlayerRotation(float dt, Player player, Vector2i mouseDelta)
        {
            if (Game.Instance.MouseVisible)
                return;

            if (mouseDelta.X != 0)
            {
                player.Direction += player.RotationSpeed * mouseDelta.X * mouseSpeedMultiplier;
            }
        }

        public static bool IsKeyPressed(Keyboard.Key key)
        {
            return Keyboard.IsKeyPressed(key);
        }
    }
}
