using SFML.Graphics;
using SFML.System;

namespace WolfRender
{
    public class FpsCounterEffect : Effect
    {
        Text fpsText;
        Font font;

        uint characterSize = 15;
        int numFrames;

        Time timeSinceLastUpdate;
        Clock clock;

        Text createText(string content, uint size, Vector2f position) => new Text(content, font)
        {
            FillColor = Color.Black,
            Position = position,
            CharacterSize = size
        };

        public FpsCounterEffect() : base("FpsCounterEffect")
        {
            clock = new Clock();
            fpsText = new Text();
            font = new Font("cour.ttf");
            timeSinceLastUpdate = Time.Zero;
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
            if (timeSinceLastUpdate.AsSeconds() > 0.5)
            {
                var windowHeight = Game.Instance.Window.Size.Y;
                var fps = numFrames/timeSinceLastUpdate.AsSeconds();
                fpsText = createText($"FPS: {fps}", characterSize, new Vector2f(0, windowHeight - characterSize - 4));
                timeSinceLastUpdate = Time.Zero;
                numFrames = 0;
            }
        }
    }
}
