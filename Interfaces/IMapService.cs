using SFML.System;

namespace WolfRender.Interfaces
{
    internal interface IMapService
    {
        int Get(Vector2i position);
    }
}