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
        Texture texture;
        uint rayNum = 20;
        float rayStep = 0.05f;
        int[] pixels;

        public MapRenderer(Map map, Player player) : base("MapRenderer")
        {
            this.map = map;
            this.player = player;
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;
            texture = new Texture(windowWidth, windowHeight);
            cellSize = new Vector2u(windowWidth / map.Size.X, windowHeight / map.Size.Y);
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;
            pixels = new int[windowWidth * windowHeight];

            var rect = new Vector2f(windowWidth / map.Size.X, windowHeight / map.Size.Y);
            for (int i = 0; i < windowWidth; i++)
            {
                var angle = player.Direction - player.Fov * 0.5f + player.Fov * i / windowWidth;
                for (float rayLength = 0; rayLength < rayNum; rayLength += rayStep)
                {
                    var playerPosition = player.Position;
                    var cx = (uint)(playerPosition.X + rayLength * MathF.Cos(angle));
                    var cy = (uint)(playerPosition.Y + rayLength * MathF.Sin(angle));
                    var pix_x = cx * rect.X;
                    var pix_y = cy * rect.Y;
                    if (map.Data[cx + cy * map.Size.X] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - player.Direction);
                        var columnHeight = Math.Min(2000, windowHeight / dist);
                        var color = Tools.PackColor((byte)(255 - Math.Clamp(rayLength * rayLength, 10, 255)), 0, 0);
                        drawRectangle(i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight, color);
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

        void drawRectangle(int x, int y, int width, int height, int color)
        {
            var windowWidth = Game.Instance.Window.Size.X;
            var windowHeight = Game.Instance.Window.Size.Y;

            if (height > windowHeight || width > windowWidth)
                return;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var cx = x + i;
                    var cy = y + j;
                    var index = cx + cy * windowWidth;
                    pixels[index] = color;
                }
            }
        }
    }
}
