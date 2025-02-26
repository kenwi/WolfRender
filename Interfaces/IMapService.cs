using SFML.System;

namespace WolfRender.Interfaces
{
    internal interface IMapService
    {
        double[] WallDistances { get; set; }
        int Get(Vector2i position);
    }
}