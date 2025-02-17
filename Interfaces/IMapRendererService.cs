using SFML.Graphics;

namespace WolfRender.Interfaces
{
    internal interface IMapRendererService
    {
        Texture MapTexture { get; }
        void Init();
        void CalculateZBuffer();
    }
}