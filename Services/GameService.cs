using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WolfRender.Interfaces;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class GameService : IGameService
    {
        private ITextureService _textureService;
        private GameConfiguration _config;
        private ILogger<GameService> _logger;
        private IMapRenderer _mapRenderer;
        private IPlayerService _playerService;
        private List<Drawable> _drawables;
        private Clock _time;

        private RenderWindow _window;
        
        public GameService(
            IOptions<GameConfiguration> config,
            ILogger<GameService> logger,
            ITextureService textureService,
            IMapRenderer mapRenderer,
            IPlayerService playerService)
        {
            _textureService = textureService;
            _config = config.Value;
            _logger = logger;
            _mapRenderer = mapRenderer;
            _playerService = playerService;
            _logger.LogInformation("GameService starting");
        }

        public void Init(IWindowService windowService)
        {
            _time = new Clock();
            _drawables = new List<Drawable>();
            _window = windowService.Window;

            _mapRenderer.Init();
            _playerService.Init(windowService);
            _drawables.Add(_mapRenderer as Drawable);

            _logger.LogInformation("GameService initialized with window service");
        }

        public void Run()
        {
            while (_window.IsOpen)
            {
                float deltaTime = _time.Restart().AsSeconds();
                _window.DispatchEvents();
                
                Update(deltaTime);
                Render();
                _window.Display();
            }
            _logger.LogInformation("GameService stopped");
        }

        public void Render()
        {
            //_window.Clear(Color.Black);
            foreach (var drawable in _drawables)
            {
                _window.Draw(drawable);
            }
        }

        public void Update(float dt)
        {            
            foreach(var drawable in _drawables)
            {
                _playerService.Update(dt);
            }
        }
    }
}
