using System;

namespace WolfRender
{
    public static class Tools
    {
        private static float PI_OVER_180 = MathF.PI / 180;
        public static float DegToRad(float radians) => radians * PI_OVER_180;

        public static int PackColor(byte r, byte g, byte b, byte a = 255)
        {
            return (a << 24) + (b << 16) + (g << 8) + r;
        }

        public static Tuple<byte, byte, byte, byte> UnpackColor(int color)
        {
            return Tuple.Create<byte, byte, byte, byte>(
                (byte)((color << 0) & 255),
                (byte)((color << 8) & 255),
                (byte)((color << 16) & 255),
                (byte)((color << 24) & 255)
            );
        }
    }
}
