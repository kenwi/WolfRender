using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;
using WolfRender.Components;
using WolfRender.Interfaces;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class GameService : IGameService
    {
        private readonly GameConfiguration _config;
        private readonly ILogger<GameService> _logger;
        private readonly IMapRenderer _mapRendererService;
        private readonly IPlayerService _playerService;
        private List<Drawable> _drawables;
        private Clock _time;

        private RenderWindow _window;
        
        public GameService(
            IOptions<GameConfiguration> config,
            ILogger<GameService> logger,
            IMapRenderer mapRendererService,
            IPlayerService playerService)
        {
            _config = config.Value;
            _logger = logger;
            _mapRendererService = mapRendererService;
            _playerService = playerService;
            _logger.LogInformation("GameService starting");
        }

        public void Init(IWindowService windowService)
        {
            _time = new Clock();
            _drawables = new List<Drawable>();
            _window = windowService.Window;

            _mapRendererService.Init();
            _playerService.Init(windowService);
            _drawables.Add(_mapRendererService as Drawable);
            _drawables.Add(new FpsTrackerComponent(_config.Resolution.Y));

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
            foreach (var drawable in _drawables)
            {
                _window.Draw(drawable);
            }
        }

        public void Update(float dt)
        {            
            // For each drawable, update if it implements IUpdateable
            foreach (var drawable in _drawables)
            {
                if (drawable is IUpdateable updateable)
                {
                    updateable.Update(dt);
                }
            }
                _playerService.Update(dt);
            }
        }
    }
