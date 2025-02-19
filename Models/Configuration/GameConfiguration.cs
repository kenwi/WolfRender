using SFML.System;
using System;

namespace WolfRender.Models.Configuration
{
    public class GameConfiguration
    {
        public Vector2i Resolution { get; set; } = new Vector2i(1024, 768);
        public uint TargetFps { get; set; }
        public float ShadingExponent { get; set; } = 5;
        public float DefaultFov { get; set; } = (float)Math.PI / 2.0f;
    }
}
