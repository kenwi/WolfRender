using SFML.System;
using System.Collections.Generic;

namespace WolfRender.Interfaces
{
    public interface IMapService
    {
        int[,] PathData { get; }
        double[] WallDistances { get; set; }
        int MapHeight { get; }
        int MapWidth { get; }

        int Get(Vector2i position);
        List<Vector2f> PathFind(Vector2i from, Vector2i to);
    }
}