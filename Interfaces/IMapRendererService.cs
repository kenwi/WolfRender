using SFML.Graphics;

namespace WolfRender.Interfaces
{
    public interface IMapRendererService
    {
        Texture MapTexture { get; }
        void Init();
        void CalculateZBuffer();
        void DebugSpritesDistance();
    }
}