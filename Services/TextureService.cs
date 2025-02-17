using Microsoft.Extensions.Logging;
using SFML.Graphics;
using System.Collections.Generic;

namespace WolfRender.Services
{
    public class TextureService : ITextureService
    {
        private readonly ILogger<TextureService> _logger;
        private readonly Dictionary<string, Texture> _textures;

        public TextureService(ILogger<TextureService> logger)
        {
            _logger = logger;
            _textures = new Dictionary<string, Texture>();
            _logger.LogInformation("TextureService starting");
        }

        public void LoadTexture(string name, string path)
        {
            var texture = new Texture(path);
            _textures.Add(name, texture);
            _logger.LogInformation($"Loaded texture ({texture.Size}) {name} from {path}");
        }

        public Texture GetTexture(string name)
        {
            return _textures[name];
        }

        public Image GetTextureImage(string name)
        {
            return GetTexture(name).CopyToImage();
        }

        public int[] GetTextureArray(string name)
        {
            var image = GetTextureImage(name);
            var width = image.Size.X;
            var height = image.Size.Y;
            var pixels = new int[width * height];

            for (uint y = 0; y < height; y++)
            {
                for (uint x = 0; x < width; x++)
                {
                    var pixelColor = image.GetPixel(x, y);
                    pixels[x + y * width] = Tools.PackColor(pixelColor.R, pixelColor.G, pixelColor.B);
                }
            }
            return pixels;
        }
    }
}
