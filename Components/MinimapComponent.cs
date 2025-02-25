using SFML.Graphics;
using SFML.System;
using System.Numerics;
using System;
using WolfRender.Interfaces;
using WolfRender.Models;

namespace WolfRender.Components
{
    internal class MinimapComponent : Drawable, IUpdateable
    {
        private IPlayer _player;
        private RectangleShape _minimapBackground;
        private Sprite _minimapSprite;
        private CircleShape _playerDot;
        private ConvexShape _playerFov;
        private float previousFov;

        private const float MINIMAP_SCALE = 4.0f;

        public MinimapComponent(Vector2i mapSize, int windowWidth, Texture mapTexture, IPlayer player)
        {
            _player = player;
            // Initialize minimap elements
            _minimapBackground = new RectangleShape(new Vector2f(mapSize.X * MINIMAP_SCALE, mapSize.Y * MINIMAP_SCALE));
            _minimapBackground.FillColor = new Color(0, 0, 0, 128);  // Semi-transparent black
            _minimapBackground.Position = new Vector2f(windowWidth - (mapSize.X * MINIMAP_SCALE) - 10, 10);  // 10px padding

            _minimapSprite = new Sprite(mapTexture);
            _minimapSprite.Scale = new Vector2f(MINIMAP_SCALE, MINIMAP_SCALE);
            _minimapSprite.Position = _minimapBackground.Position;
            _minimapSprite.Color = new Color(255, 255, 255, 64);  // Semi-transparent white

            _playerDot = new CircleShape(2);  // 3px radius
            _playerDot.FillColor = Color.Red;

            _playerFov = new ConvexShape(3);  // Triangle shape
            _playerFov.FillColor = new Color(0, 0, 128, 64);
            UpdateFovCone();
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_minimapBackground);
            target.Draw(_minimapSprite);
            target.Draw(_playerDot);
            target.Draw(_playerFov);
        }

        private void UpdateFovCone()
        {
            float distanceToEdge = 20.0f;

            // Calculate the width of the triangle based on the FoV
            float width = 2 * distanceToEdge * (float)Math.Tan(_player.Fov / 2.0f);

            // Set the points of the triangle
            _playerFov.SetPoint(0, new Vector2f(0, 0)); // Apex of the triangle
            _playerFov.SetPoint(1, new Vector2f(-width / 2, -distanceToEdge)); // Left point
            _playerFov.SetPoint(2, new Vector2f(width / 2, -distanceToEdge)); // Right point
        }

        public void Update(float time)
        {
            _playerFov.Position = new Vector2f(
                _minimapSprite.Position.X + (_player.Position.X * MINIMAP_SCALE),
                _minimapSprite.Position.Y + (_player.Position.Y * MINIMAP_SCALE));
            _playerFov.Rotation = (float)(_player.Direction * 180.0f / (float)Math.PI + 90.0f);  // Convert to degrees and offset
            _playerDot.Position = new Vector2f(
                _minimapSprite.Position.X + (_player.Position.X * MINIMAP_SCALE) - _playerDot.Radius,
                _minimapSprite.Position.Y + (_player.Position.Y * MINIMAP_SCALE) - _playerDot.Radius);
            
            if (_player.Fov != previousFov)
            {
                UpdateFovCone();
                previousFov = _player.Fov;
            }
        }
    }
}
