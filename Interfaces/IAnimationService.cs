using SFML.Graphics;

namespace WolfRender.Interfaces
{
    public interface IAnimationService
    {
        void LoadSpriteSheet(string name, string path);
        void CreateAnimation(string sheetName, string animationName, int row, int framesCount);
        Sprite GetSprite(string sheetName, string animationName, int angleIndex);
        Sprite GetSprite(string sheetName, string animationName, float angle);
        Sprite GetSpriteForRelativeAngle(string sheetName, string animationName, float relativeAngle);
    }
} 