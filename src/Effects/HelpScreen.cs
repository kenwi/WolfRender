using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class HelpScreen : Effect
    {
        Text text;
        Font font;
        Clock clock;
        int duration = 5;

        public HelpScreen() : base("HelpScreen")
        {
            clock = new Clock();
            font = new Font("cour.ttf");
            text = new Text("", font, 15);
            // text.DisplayedString += $"[Up/Down] Move Forward/Backward\n";
            // text.DisplayedString += $"[Left/Right] Rotate Left/Right\n";
            text.DisplayedString += $"[WASD] Movement\n";
            text.DisplayedString += $"[Page Up/Down] Adjust FOV\n";
            text.DisplayedString += $"[L] Toggle Framerate Limiting\n";
            text.DisplayedString += $"[H] Toggle Help\n";
            text.DisplayedString += $"[Escape] Quit\n";
            text.FillColor = Color.Green;
            Game.Instance.HelpMenuVisible = true;
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            if (Game.Instance.HelpMenuVisible)
            {
                target.Draw(text, states);
            }
        }

        protected override void OnUpdate(float time)
        {
            if (clock?.ElapsedTime.AsSeconds() > duration)
            {
                Game.Instance.HelpMenuVisible = false;
                clock = null;
            }
        }
    }
}
