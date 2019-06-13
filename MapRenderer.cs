using System;
using SFML.System;
using SFML.Graphics;

namespace WolfRender
{
    public class MapRenderer : Effect
    {
        Map map;
        Player player;
        uint rayNum = 20;
        float rayStep = 0.1f;
        int[] pixels;

        public MapRenderer(Map map, Player player) : base("MapRenderer")
        {
            this.map = map;
            this.player = player;
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            var fov = Game.Instance.Player.Fov;
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;
            var mapSizeX = map.Size.X;
            var mapSizeY = map.Size.Y;
            var rect_w = windowWidth / mapSizeX;
            var rect_h = windowHeight / mapSizeY;

            var rect = new Vector2f(windowWidth / mapSizeX, windowHeight / mapSizeY);
            for (int i = 0; i < windowWidth; i++)
            {
                var playerDirection = Game.Instance.Player.Direction;
                var angle = playerDirection - fov * 0.5f + fov * i / windowWidth;
                for (float rayLength = 0; rayLength < rayNum; rayLength += rayStep)
                {
                    var playerPosition = Game.Instance.Player.Position;
                    var cx = (uint)(playerPosition.X + rayLength * MathF.Cos(angle));
                    var cy = (uint)(playerPosition.Y + rayLength * MathF.Sin(angle));
                    var pix_x = cx * rect_w;
                    var pix_y = cy * rect_h;

                    if (map.Data[cx + cy * mapSizeX] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - playerDirection);
                        var columnHeight = Math.Min(2000, windowHeight / dist);
                        // var columnHeight = windowHeight / (rayLength * Math.Cos(angle - playerDirection));
                        var color = Tools.PackColor((byte)(255 - Math.Clamp(rayLength * rayLength, 10, 255)), 0, 0);
                        drawRectangle(pixels, windowWidth, windowHeight, i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight, color);
                        break;
                    }
                }
            }
        }

        protected override void OnUpdate(float time)
        {

        }

        void drawRectangle(int[] img, uint width, uint height, int x, int y, int w, int h, int color)
        {
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;

            if (h > windowHeight || w > windowWidth)
                return;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var cx = x + i;
                    var cy = y + j;
                    var index = cx + cy * width;
                    img[index] = color;
                }
            }
        }
    }
}
