using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace WolfRender.Components
{
    internal class CrosshairComponent : Drawable
    {
        private readonly Vector2i _windowResolution;
        private readonly List<RectangleShape> _crosshairLines = new List<RectangleShape>();

        public CrosshairComponent(Vector2i windowResolution)
        {
            var crosshairColor = new Color(255, 253, 208);
            _windowResolution = windowResolution;
            _crosshairLines.Add(new RectangleShape(new Vector2f(10, 1))
            {
                Position = new Vector2f(windowResolution.X / 2 - 5, windowResolution.Y / 2),
                FillColor = crosshairColor,
                OutlineColor = crosshairColor,
                OutlineThickness = 0.5f,
            });
            _crosshairLines.Add(new RectangleShape(new Vector2f(1, 10))
            {
                Position = new Vector2f(windowResolution.X / 2, windowResolution.Y / 2 - 5),
                FillColor = crosshairColor,
                OutlineColor = crosshairColor,
                OutlineThickness = 0.5f
            });
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            _crosshairLines.ForEach(line => target.Draw(line));
        }
    }
}
