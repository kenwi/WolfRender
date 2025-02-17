using Microsoft.Extensions.Logging;
using SFML.System;
using System;
using System.Drawing;
using WolfRender.Interfaces;

namespace WolfRender.Services
{
    internal class MapService : IMapService
    {
        private ILogger<MapService> _logger;
        private ITextureService _textureService;
        private int[,] _data { get; set; }
        public Vector2i Size { get; set; }

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
            _textureService.LoadTexture("map", "map.bmp");
            _data = _textureService.GetMapData("map");
            //_data = _textureService.GetTextureArray("map");
            
            var textureSize = (int)Math.Sqrt(_data.Length);
            Size = new Vector2i(textureSize, textureSize);
            _logger.LogInformation("MapService initialized");
        }

        public int Get(Vector2i position)
        {
            if (position.X < 0 || position.X >= Size.X ||
                position.Y < 0 || position.Y >= Size.Y)
                return 1; // Return wall for out of bounds

            return _data[position.X, position.Y];
        }
    }
}
