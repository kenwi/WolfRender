namespace WolfRender.Interfaces
{
    public interface IPlayerService
    {
        IPlayer Player { get; }
        void Update(float deltaTime);
        void Init(IWindowService windowService, IMapRendererService mapRendererService);
    }
}