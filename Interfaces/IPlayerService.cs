using SFML.Graphics;

namespace WolfRender.Interfaces
{
    public interface IPlayerService
    {
        Sprite CurrentWeaponSprite { get; }
        IPlayer Player { get; }
        void Update(float deltaTime);
        void Init(IWindowService windowService, IMapRendererService mapRendererService, IEntityService _entityService);
    }
}