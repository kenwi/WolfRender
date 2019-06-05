using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WolfRender
{
    public class HelpScreenEffect : Effect
    {
        Text text;
        Font font;
        Clock clock;
        int duration = 5;
        public bool Visible { get; set; } = true;

        public HelpScreenEffect() : base("HelpScreenEffect")
        {
            clock = new Clock();
            font = new Font("cour.ttf");
            text = new Text("[Up/Down] Move Forward/Backward\n[Left/Right] Rotate Left/Right\n[Page Up/Down] Adjust FOV\n[Escape] Quit", font, 15);
            text.FillColor = Color.Black;
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            if (Visible)
            {
                target.Draw(text, states);
            }
        }

        protected override void OnUpdate(float time)
        {
            if (clock?.ElapsedTime.AsSeconds() > duration)
            {
                Visible = false;
                clock = null;
            }
        }
    }
}
