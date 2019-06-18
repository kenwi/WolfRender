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
        Vector2u textureSize;
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
            textureSize = new Vector2u((uint)Math.Sqrt(texturePixels.Length), (uint)Math.Sqrt(texturePixels.Length));
            bytes = new byte[windowWidth * windowHeight * 4];
            sprite = new Sprite(texture);
        }

        int GetWallTextureID(double hitx, double hity, int[] tex_walls)
        {
            double x = hitx - Math.Floor(hitx + 0.5f);
            double y = hity - Math.Floor(hity + 0.5f);
            int tex = (int)(x * Math.Sqrt(tex_walls.Length));

            if (Math.Abs(y) > Math.Abs(x))
                tex = (int)(y * Math.Sqrt(tex_walls.Length));
            if (tex < 0)
                tex += (int)Math.Sqrt(tex_walls.Length);

            return tex;
        }

        int[] GetScaledColumn(int textureId, int textureCoord, int columnHeight, int[] texture)
        {
            int[] columnPixels = new int[columnHeight];
            for (int y = 0; y < columnHeight; y++)
            {
                columnPixels[y] = GetTextureValue(textureCoord, (int)(y * textureSize.Y) / columnHeight, textureId, textureSize.X);
            }
            return columnPixels;
        }

        int GetTextureValue(int x, int y, int idx, uint size) => texturePixels[x + idx * size + y * size];

        int[] texturePixels = {
            1, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1, 1, 1,
        };

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
                    
                    int wallTextureID = GetWallTextureID(cellX, cellY, texturePixels);
                    if (map.Data[(int)cellX + (int)cellY * map.Size.X] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - player.Direction);
                        var columnHeight = (int)Math.Min(2000, windowHeight / dist);                        
                        var column = GetScaledColumn(0, wallTextureID, columnHeight, texturePixels);
                        drawRectangle(i, (int)windowHeight / 2 - columnHeight / 2, 1, columnHeight, column, rayLength);
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

        void drawRectangle(int x, int y, int w, int h, int[] color, double distance)
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
                    var colorId = color[j];
                    if(colorId != 0)
                        pixels[index] = Tools.PackColor((byte)(150 - Math.Clamp(distance, 1, 128)), 0, 0);
                    else
                        pixels[index] = Tools.PackColor(0, 0, (byte)(150 - Math.Clamp(distance * distance, 1, 128)));
                }
            }
        }
    }
}
