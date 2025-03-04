using Microsoft.Extensions.Logging;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
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
            _entities = new List<IEntity>
            {
                new StaticEntity(_textureService, "barrel")
                {
                    Position = new Vector2f(20.5f, 46.5f),
                    Direction = 0
                },
                new StaticEntity(_textureService, "barrel")
                {
                    Position = new Vector2f(20.5f, 44.5f),
                    Direction = 0
                },
                new AnimatedEntity(_animationService, "guard")
                {
                    Position = new Vector2f(20.5f, 45.5f),
                    Direction = 0
                }
            };
        }

        public void Update(float dt)
        {
            // Update all entities
            foreach (var entity in _entities)
            {
                if (entity is AnimatedEntity animatedEntity)
                {
                    if (!entity.IsAlive)
                        break;

                    animatedEntity.Update(dt);

                    if (animatedEntity.IsAnimating)
                        break;

                    // If the entity is following a path, continue that
                    if (animatedEntity.IsFollowingPath)
                    {
                        animatedEntity.WalkPath(dt); // Pass null to continue current path
                        continue;
                    }

                    // Direction controls (in radians)
                    if (Input.IsKeyPressed(Keyboard.Key.Num1))
                        animatedEntity.Direction = 0;           // East
                    else if (Input.IsKeyPressed(Keyboard.Key.Num2))
                        animatedEntity.Direction = (float)Math.PI / 2; // South
                    else if (Input.IsKeyPressed(Keyboard.Key.Num3))
                        animatedEntity.Direction = (float)Math.PI;     // West
                    else if (Input.IsKeyPressed(Keyboard.Key.Num4))
                        animatedEntity.Direction = (float)-Math.PI /2;  // North
                    else if (Input.IsKeyPressed(Keyboard.Key.Num5))
                        animatedEntity.Direction += 0.5f * dt; // Rotate right

                    // Animation controls
                    if (Input.IsKeyPressed(Keyboard.Key.E))
                    {
                        animatedEntity.Walk(dt);
                    }
                    else if (!animatedEntity.IsFollowingPath)
                    {
                        animatedEntity.SetAnimation("idle");
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.Q))
                    {
                        animatedEntity.SetAnimation("attack");
                        animatedEntity.IsAnimating = true;
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.T))
                    {
                        animatedEntity.SetAnimation("death");
                        animatedEntity.IsAnimating = true;
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.R))
                    {
                        animatedEntity.SetAnimation("hit");
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.Space))
                    {
                        Vector2i fromPos = new Vector2i((int)entity.Position.X, (int)entity.Position.Y);
                        Vector2i toPos = new Vector2i((int)_playerService.Player.Position.X, (int)_playerService.Player.Position.Y); //new Vector2i(10, 50);  // Example destination

                        var path = _mapService.PathFind(fromPos, toPos);
                        if (path != null)
                        {
                            animatedEntity.WalkPath(path, dt);
                        }
                    }
                }
            }
        }
    }
}
