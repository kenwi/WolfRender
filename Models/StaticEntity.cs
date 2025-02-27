using SFML.Graphics;
using SFML.System;
using WolfRender.Interfaces;
using WolfRender.Services;

namespace WolfRender.Models
{
    public class StaticEntity : IEntity
    {
        private Texture _texture;
        private Sprite _sprite;
        private Vector2f _position;
        private float _direction;

        public Sprite Sprite
        {
            get => _sprite;
            set => _sprite = value;
        }

        public Vector2f Position
        {
            get => _position;
            set
            {
                _position = value;
                _sprite.Position = value;
            }
        }

        public float Direction
        {
            get => _direction;
            set
            {
                _direction = value;
            }
        }

        public Texture Texture
        {
            get => _texture;
        }
        

        public StaticEntity(ITextureService textureService, string textureName)
        {
            var image = textureService.GetTextureImage(textureName);
            image.CreateMaskFromColor(Color.Black);
            _texture = new Texture(image);
            _sprite = new Sprite(_texture);
            _sprite.Origin = new Vector2f(_texture.Size.X / 2, _texture.Size.Y / 2);
            _sprite.Position = Position;
        }
    }
}
