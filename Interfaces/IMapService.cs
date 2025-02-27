using SFML.System;

namespace WolfRender.Interfaces
{
    public interface IMapService
    {
        double[] WallDistances { get; set; }
        int Get(Vector2i position);
    }
}