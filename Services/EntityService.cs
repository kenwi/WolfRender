using Microsoft.Extensions.Logging;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using WolfRender.Interfaces;
using WolfRender.Models;

namespace WolfRender.Services
{
    public class EntityService : IEntityService
    {
        private List<IEntity> _entities;
        private ITextureService _textureService;
        private IAnimationService _animationService;
        private IMapService _mapService;
        private IPlayerService _playerService;

        public List<IEntity> Entities => _entities;

        public EntityService(
            ILogger<EntityService> logger,
            ITextureService textureService,
            IAnimationService animationService,
            IMapService mapService,
            IPlayerService playerService)
        {
            _textureService = textureService;
            _animationService = animationService;
            _mapService = mapService;
            _playerService = playerService;
        }

        public void Init()
        {
            CreateEntitiesFromPathData();
        }

        public void Update(float dt)
        {
            foreach (var entity in _entities)
            {
                if (entity is AnimatedEntity animatedEntity)
                {
                    animatedEntity.Update(dt);
                }
            }

        }

        public void AddEntity(IEntity entity)
        {
            _entities.Add(entity);
        }

        public void CreateEntitiesFromPathData()
        {
            var data = _mapService.PathData;

            // Create entities from map data
            for (int x = 0; x < _mapService.MapWidth; x++)
            {
                for (int y = 0; y < _mapService.MapHeight; y++)
                {
                    var id = data[y, x];
                    IEntity entity = null;
                    switch (id)
                    {
                        case (int)Models.EntityType.Guard:
                            entity = new AnimatedEntity(_animationService, "guard", _mapService, _playerService)
                            {
                                Position = new Vector2f(y + 0.5f, x + 0.5f) // Coordinates are flipped
                            };
                            break;
                        case (int)Models.EntityType.Barrel:
                            entity = new StaticEntity(_textureService, "barrel")
                            {
                                Position = new Vector2f(y + 0.5f, x + 0.5f)
                            };
                            break;
                    }

                    if (entity != null)
                    {
                        AddEntity(entity);
                    }   
                }
            }
        }
    
    }
}
