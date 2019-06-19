using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class HelpScreen : Effect
    {
        Text text;
        Font font;
        Time duration = Time.FromSeconds(5);
        bool active = true;

        public HelpScreen() : base("HelpScreen")
        {
            font = new Font("cour.ttf");
            text = new Text("", font, 15);
            text.DisplayedString += $"[H] Toggle Help\n";
            text.DisplayedString += $"[M] Toggle Mouse\n";
            text.DisplayedString += $"[L] Toggle Framerate Limiting\n";
            text.DisplayedString += $"[PageUp/PageDown] Adjust FOV\n";
            text.DisplayedString += $"[WASD+ArrowUp/ArrowDown] Movement\n";
            text.DisplayedString += $"[Left/Right] Rotation\n";
            text.DisplayedString += $"[Escape] Quit\n";
            text.FillColor = Color.Green;
            Instance.IsHelpMenuVisible = true;
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            if (Instance.IsHelpMenuVisible)
            {
                target.Draw(text, states);
            }
        }

        protected override void OnUpdate(float time)
        {
            if(active && Instance.TotalGameTime > duration)
            {
                Instance.IsHelpMenuVisible = false;
                active = false;
            }
        }
    }
}
