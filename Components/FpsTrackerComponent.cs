using SFML.Graphics;
using SFML.System;
using WolfRender.Interfaces;

namespace WolfRender.Components
{
    internal class FpsTrackerComponent : Drawable, IUpdateable
    {
        private int _numFrames;
        private readonly uint _characterSize = 15;

        private Text _fpsText = new Text();
        private Font _font = new Font("cour.ttf");

        private Time _updateRate = Time.FromSeconds(0.5f);
        private Time _timeSinceLastUpdate = Time.Zero;
        private Clock clock = new Clock();
        private int _windowHeight;

        public FpsTrackerComponent(int windowHeight)
        {
            _windowHeight = windowHeight;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _numFrames++;
            target.Draw(_fpsText, states);
        }

        private void SetText(string content, uint size, Vector2f position)
        {
            var text = new Text(content, _font)
            {
                FillColor = Color.White,
                Position = position,
                CharacterSize = size,
                OutlineThickness = 1,
                Style = Text.Styles.Bold
            };
            _fpsText = text;
        }

        public void Update(float dt)
        {
            Time elapsed = clock.Restart();
            _timeSinceLastUpdate += elapsed;
            if (_timeSinceLastUpdate > _updateRate)
            {
                var fps = _numFrames / _timeSinceLastUpdate.AsSeconds();
                _timeSinceLastUpdate = Time.Zero;
                _numFrames = 0;
                SetText($"[{fps:0.#} FPS / {dt:0.#######} ms]",
                    _characterSize,
                    new Vector2f(0, _windowHeight - _characterSize - 4));
            }
        }
    }
}
