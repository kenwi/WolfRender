using SFML.Graphics;
using SFML.System;

namespace WolfRender
{
    public class FpsCounter : Effect
    {
        int numFrames;
        uint characterSize = 15;

        Text fpsText = new Text();
        Font font = new Font("cour.ttf");

        Time updateRate = Time.FromSeconds(0.5f);
        Time timeSinceLastUpdate = Time.Zero;
        Clock clock = new Clock();

        public FpsCounter() : base("FpsCounterEffect")
        {
        }

        void setText(string content, uint size, Vector2f position)
        {
            var text = new Text(content, font)
            {
                FillColor = Color.Green,
                Position = position,
                CharacterSize = size
            };
            fpsText = text;
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
            if (timeSinceLastUpdate > updateRate)
            {
                var windowHeight = Game.Instance.Window.Size.Y;
                var fps = numFrames / timeSinceLastUpdate.AsSeconds();
                timeSinceLastUpdate = Time.Zero;
                numFrames = 0;
                setText($"[ {fps:0.#} FPS / {Game.Instance.DeltaTime:0.#######} ms ]", characterSize, new Vector2f(0, windowHeight - characterSize - 4));
            }
        }
    }
}
