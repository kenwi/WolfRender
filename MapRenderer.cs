using System;
using SFML.System;
using SFML.Graphics;

namespace WolfRender
{
    public class MapRenderer : Effect
    {
        Map map;
        Player player;
        Vector2u cellSize;
        uint rayNum = 20;
        float rayStep = 0.01f;
        int[] pixels;

        Texture texture;

        public MapRenderer(Map map, Player player) : base("MapRenderer")
        {
            this.map = map;
            this.player = player;
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;
            texture = new Texture(windowWidth, windowHeight);
            cellSize = new Vector2u(windowWidth / map.Size.X, windowHeight / map.Size.Y);
            pixels = new int[windowWidth * windowHeight];
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            var fov = player.Fov;
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;
            var mapSizeX = map.Size.X;
            var mapSizeY = map.Size.Y;
            var rect_w = windowWidth / mapSizeX;
            var rect_h = windowHeight / mapSizeY;

            var rect = new Vector2f(windowWidth / mapSizeX, windowHeight / mapSizeY);
            for (int i = 0; i < windowWidth; i++)
            {
                var playerDirection = player.Direction;
                var angle = playerDirection - fov * 0.5f + fov * i / windowWidth;
                for (float rayLength = 0; rayLength < rayNum; rayLength += rayStep)
                {
                    var playerPosition = player.Position;
                    var cx = (uint)(playerPosition.X + rayLength * MathF.Cos(angle));
                    var cy = (uint)(playerPosition.Y + rayLength * MathF.Sin(angle));
                    var pix_x = cx * rect_w;
                    var pix_y = cy * rect_h;

                    if (map.Data[cx + cy * mapSizeX] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - playerDirection);
                        var columnHeight = Math.Min(2000, windowHeight / dist);
                        var color = Tools.PackColor((byte)(255 - Math.Clamp(rayLength * rayLength, 10, 255)), 0, 0);
                        drawRectangle(pixels, windowWidth, windowHeight, i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight, color);
                        break;
                    }
                }
            }

            var bytes = new byte[windowWidth * windowHeight * 4];
            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            texture.Update(bytes);
            var sprite = new Sprite(texture);
            target.Draw(sprite, states);
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
