namespace WolfRender.Interfaces
{
    internal interface IGameService
    {
        void Init(IWindowService windowService);
        void Run();
        void Render();
        void Update(float dt);
    }
}