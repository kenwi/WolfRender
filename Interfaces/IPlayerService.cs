namespace WolfRender.Interfaces
{
    internal interface IPlayerService
    {
        IPlayer Player { get; }
        void Update(float deltaTime);
        void Init(IWindowService windowService);
    }
}