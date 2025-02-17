using SFML.System;

namespace WolfRender.Models.Configuration
{
    public class GameConfiguration
    {
        public Vector2i Resolution { get; set; } = new Vector2i(1024, 768);
        public uint TargetFps { get; set; }
    }
}
