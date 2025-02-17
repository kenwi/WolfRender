using Microsoft.Extensions.Logging;
using SFML.System;
using System;
using System.Drawing;
using WolfRender.Interfaces;

namespace WolfRender.Services
{
    internal class MapService : IMapService
    {
        private readonly ILogger<MapService> _logger;
        private readonly ITextureService _textureService;
        private int[,] _data { get; set; }
        private Vector2i _size;

        public MapService(
            ILogger<MapService> logger,
            ITextureService textureService)
        {
            _logger = logger;
            _textureService = textureService;
            _logger.LogInformation("MapService starting");

            Init();
        }

        private void Init()
        {
            _data = GetMapData("map");
            
            var textureSize = (int)Math.Sqrt(_data.Length);
            _size = new Vector2i(textureSize, textureSize);
            _logger.LogInformation("MapService initialized");
        }

        public int Get(Vector2i position)
        {
            if (position.X < 0 || position.X >= _size.X ||
                position.Y < 0 || position.Y >= _size.Y)
                return 1; // Return wall for out of bounds

            return _data[position.X, position.Y];
        }

        private int[,] GetMapData(string name)
        {
            _textureService.LoadTexture(name, $"{name}.bmp");
            var image = _textureService.GetTextureImage(name);
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
