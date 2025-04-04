﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Threading;
using System.Threading.Tasks;
using WolfRender.Interfaces;
using WolfRender.Models.Configuration;

namespace WolfRender.Services
{
    internal class WindowService : IHostedService, IWindowService
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly WindowConfiguration _config;
        private readonly GameConfiguration _gameConfig;
        private readonly ILogger<WindowService> _logger;
        private readonly IGameService _gameService;
        
        private RenderWindow _window;
        private Vector2i _windowCenter;
        private bool _mouseVisible;

        public RenderWindow Window => _window;
        public Vector2i WindowCenter => _windowCenter;

        public WindowService(
            IHostApplicationLifetime applicationLifetime,
            IOptions<WindowConfiguration> config,
            IOptions<GameConfiguration> gameConfig,
            ILogger<WindowService> logger,
            IGameService gameService)
        {
            _applicationLifetime = applicationLifetime;
            _config = config.Value;
            _gameConfig = gameConfig.Value;
            _logger = logger;
            _gameService = gameService;
        }

        public void CreateWindow()
        {
            var view = new View(new FloatRect(0, 0, _gameConfig.Resolution.X, _gameConfig.Resolution.Y));
            _window = new RenderWindow(new VideoMode(_config.Width, _config.Height), _config.Title);
            _window.SetView(view);
            _windowCenter = _window.Position + new Vector2i(_gameConfig.Resolution.X / 2, _gameConfig.Resolution.Y / 2);
            _window.SetVerticalSyncEnabled(_config.IsVsyncEnabled);

            _logger.LogInformation("Created Window");
        }

        private void RegisterEvents()
        {
            _window.Closed += (sender, e) =>
            {
                StopAsync(CancellationToken.None);
                _logger.LogInformation("Window Closed");
            };
            _logger.LogInformation("Registered Window Events");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CreateWindow();
            RegisterEvents();

            _gameService.Init(this);
            _gameService.Run();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _window.Close();
            _applicationLifetime.StopApplication();

            return Task.CompletedTask;
        }

        public bool IsMouseVisible
        {
            get => _mouseVisible;
            set
            {
                _window.SetMouseCursorVisible(value);
                _window.SetMouseCursorGrabbed(!value);
                _mouseVisible = value;
            }
        }

        public bool IsVsyncEnabled
        {
            get => _config.IsVsyncEnabled;
            set
            {
                _config.IsVsyncEnabled = value;
                _window.SetVerticalSyncEnabled(value);
            }
        }
    }
}
