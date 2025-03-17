using SFML.Graphics;

namespace WolfRender.Interfaces
{
    public interface IAnimationService
    {
        int CurrentFrameIndex { get;}
        void LoadSpriteSheet(string name, string path);
        void CreateAnimation(string sheetName, string animationName, int row, int framesCount);
        Sprite GetSprite(string sheetName, string animationName, int angleIndex);
        Sprite GetSprite(string sheetName, string animationName, float angle);
        Sprite GetSpriteForRelativeAngle(string sheetName, string animationName, float relativeAngle);
        void CreateMultiRowAnimation(string sheetName, string animationName, int startRow, int rowCount, int framesPerRow, int paddingX = 0, int paddingY = 0);
        Sprite GetAnimationFrame(string sheetName, string animationName, int angleIndex, float animationTime, float frameRate);
        Sprite GetAnimationFrame(string sheetName, string animationName, float animationTime, float frameRate);
    }
} 