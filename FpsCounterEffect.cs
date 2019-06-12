using SFML.Graphics;
using SFML.System;

namespace WolfRender
{
    public class FpsCounterEffect : Effect
    {
        Text fpsText;
        Font font;
        int numFrames;
        Time timeSinceLastUpdate;
        Clock clock;
        int padding;

        Text createText(string content, uint size, Vector2f position)
        {
            Text text = new Text(content, font, size);
            text.FillColor = Color.Black;
            text.Position = new Vector2f(padding, Game.Instance.Window.Size.Y - text.CharacterSize - padding);
            return text;
        }

        public FpsCounterEffect() : base("FpsCounterEffect")
        {
            clock = new Clock();
            timeSinceLastUpdate = Time.Zero;
            uint characterSize = 15;
            padding = 4;
            font = new Font("cour.ttf");
            fpsText = createText("Hello World", characterSize, new Vector2f(0, Game.Instance.Window.Size.Y));
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            numFrames++;
            target.Draw(fpsText, states);
        }

        protected override void OnUpdate(float time)
        {
            Time elapsed = clock.Restart();
            timeSinceLastUpdate += elapsed;
        }
    }
}
