using Microsoft.Extensions.Logging;
using SFML.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace WolfRender.Services
{
    public class TextureService : ITextureService
    {
        private ILogger<TextureService> _logger;
        private Dictionary<string, Texture> _textures;

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

        public int[,] GetMapData(string name)
        {
            var image = GetTextureImage(name);
            var width = image.Size.X;
            var height = image.Size.Y;
            int[,] data = new int[width, height];

            // Convert each pixel to map data
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    var pixel = image.GetPixel((uint)x, (uint)y);

                    // Convert pixel to grayscale and threshold
                    float brightness = (pixel.R + pixel.G + pixel.B) / (3.0f * 255.0f);

                    // If pixel is darker than 50% gray, it's a wall
                    data[x, y] = brightness < 0.5f ? 1 : 0;
                }
            }
            return data;
        }
    }
}
