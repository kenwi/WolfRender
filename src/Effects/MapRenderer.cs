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
        byte[] bytes;
        Sprite sprite;
        uint windowWidth, windowHeight;

        public MapRenderer(Map map, Player player) : base("MapRenderer")
        {
            this.map = map;
            this.player = player;
            windowWidth = Instance.Window.Size.X;
            windowHeight = Instance.Window.Size.Y;
            texture = new Texture(windowWidth, windowHeight);
            cellSize = new Vector2u(windowWidth / map.Size.X, windowHeight / map.Size.Y);
            bytes = new byte[windowWidth * windowHeight * 4];
            sprite = new Sprite(texture);
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            pixels = new int[windowWidth * windowHeight];
            for (int i = 0; i < windowWidth; i++)
            {
                double angle = player.Direction - player.Fov * 0.5f + player.Fov * i / windowWidth;
                for (double rayLength = 0; rayLength < rayNum; rayLength += rayStep)
                {
                    double cellX = player.Position.X + rayLength * Math.Cos(angle);
                    double cellY = player.Position.Y + rayLength * Math.Sin(angle);
                    if(map.Data[(int)cellX + (int)cellY * map.Size.X] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - player.Direction);
                        var columnHeight = (int)Math.Min(2000, windowHeight / dist);
                        var color = Tools.PackColor((byte)(255 - Math.Clamp(rayLength * rayLength, 10, 255)), 0, 0);
                        drawRectangle(i, (int)windowHeight / 2 - columnHeight / 2, 1, columnHeight, color);
                        break;
                    }
                }
            }

            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            texture.Update(bytes);
            target.Draw(sprite, states);
        }

        protected override void OnUpdate(float time)
        {

        }

        void drawRectangle(int x, int y, int w, int h, int color)
        {
            if (h > windowHeight || w > windowWidth)
                return;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
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
