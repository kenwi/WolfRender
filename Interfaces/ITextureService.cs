using SFML.Graphics;

namespace WolfRender.Services
{
    public interface ITextureService
    {
        void LoadTexture(string name, string path);
        Texture GetTexture(string name);
        Image GetTextureImage(string name);
        int[] GetTextureArray(string name);
    }
}