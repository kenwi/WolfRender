using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WolfRender.Interfaces;
using WolfRender.Services;

namespace WolfRender
{
    class Program
    {
        static void Main(string[] args)
        {
            //Game.Instance.Init(1920, 1080);
            //Input.Instance.Init();
            //Game.Instance.Run();

            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IPlayerService, PlayerService>();
                    services.AddSingleton<ITextureService, TextureService>();
                    services.AddSingleton<IGameService, GameService>();
                    services.AddSingleton<IMapRendererService, TexturedMapRendererService>();
                    services.AddSingleton<ISpriteRendererService, SpriteRenderService>();
                    services.AddSingleton<IMapService, MapService>();
                    services.AddSingleton<IAnimationService, AnimationService>();
                    services.AddSingleton<IEntityService, EntityService>();

                    services.AddHostedService<WindowService>();
                })
                .Build()
                .Run();
        }
    }
}