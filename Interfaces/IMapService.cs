using SFML.System;

namespace WolfRender.Interfaces
{
    internal interface IMapService
    {
        Vector2i Size { get; set; }
        int Get(Vector2i position);
    }
}