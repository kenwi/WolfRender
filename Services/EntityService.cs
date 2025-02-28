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
        bool _isAnimating = false;

        public List<IEntity> Entities => _entities;

        public EntityService(
            ILogger<EntityService> logger,
            ITextureService textureService,
            IAnimationService animationService)
        {
            _textureService = textureService;
            _animationService = animationService;
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
                    animatedEntity.Update(dt);

                    if (_isAnimating)
                        break;

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
                        var walkSpeed = 0.8f;
                        animatedEntity.SetAnimation("walk");
                        animatedEntity.Position += new Vector2f(walkSpeed * (float)Math.Cos(animatedEntity.Direction),
                            walkSpeed * -(float)Math.Sin(animatedEntity.Direction)) * dt;
                    }
                    else
                    {
                        animatedEntity.SetAnimation("idle");
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.Q))
                    {
                        animatedEntity.SetAnimation("attack");
                        _isAnimating = true;
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.T))
                    {
                        animatedEntity.SetAnimation("death");
                        _isAnimating = true;
                    }

                    if (Input.IsKeyPressed(Keyboard.Key.R))
                    {
                        animatedEntity.SetAnimation("hit");
                    }
                }
            }
        }
    }
}
