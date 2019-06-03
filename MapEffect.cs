using System;
using SFML.Window;
using SFML.Graphics;
using System.Numerics;

namespace WolfRender
{
    public class MapEffect : Effect
    {
        byte[] map ={
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1,
            1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        };

        byte[] palette = {
            0xFF, 0x77, 0x00,
            0xAB, 0xBB, 0xBB
        };

        int[] pixels;
        int MapSize => (int)Math.Sqrt(map.Length);
        int windowWidth => (int)Game.Instance.Window.Size.X;
        int windowHeight => (int)Game.Instance.Window.Size.Y;
        float fov;
        bool updated;
        Vector2 cellSize;
        Texture texture;
        Sprite sprite;

        Vector2 playerPosition;
        float playerDirection;

        public MapEffect() : base("MapTestEffect")
        {
            texture = new Texture(Game.Instance.Window.Size.X, Game.Instance.Window.Size.Y);
            cellSize = new Vector2(windowWidth / MapSize, windowHeight / MapSize);
            pixels = new int[windowWidth * windowHeight];

            fov = 85;
            playerDirection = 3.1415f / 2;
            playerPosition = new Vector2(MapSize / 4);

            updated = true;
            render();
            updateFrame();
        }

        void render()
        {
            int cellId = 0;
            for (int y = 0; y < MapSize; y++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    var cellColorId = map[cellId++];
                    var cellColor = palette.AsSpan(cellColorId * 3, 3);
                    drawRectangle(pixels, windowWidth, windowHeight, x * (int)cellSize.X, y * (int)cellSize.Y, (int)cellSize.X, (int)cellSize.Y, pack_color(cellColor[0], cellColor[1], cellColor[2]));
                }
            }
            raycast(playerPosition, playerDirection - (float)Math.Sin(3.1415 / 180 * fov * 0.5), true);
            raycast(playerPosition, playerDirection + (float)Math.Sin(3.1415 / 180 * fov * 0.5), true);
            drawEntity(playerPosition, 5, pack_color(0, 0, 255));
        }

        void updateFrame()
        {
            var bytes = new byte[windowWidth * windowHeight * 4];
            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            texture.Update(bytes);
            sprite = new Sprite(texture);
        }

        void setPixel(Vector2 position, int color)
        {
            int x = (int)(position.X * cellSize.X);
            int y = (int)(position.Y * cellSize.Y);
            int index = x + y * windowWidth;
            pixels[index] = color;
        }

        float raycast(Vector2 position, float direction, bool render = false)
        {
            float length = 0, step = 0.05f;
            for (; length < 20; length += step)
            {
                var dx = position.X + length * Math.Cos(direction);
                var dy = position.Y + length * Math.Sin(direction);
                if (map[(int)dx + (int)dy * MapSize] != 0)
                    break;
                if (render)
                    setPixel(new Vector2((float)dx, (float)dy), pack_color(255, 0, 0));
            }
            return length;
        }

        void drawEntity(Vector2 position, int size, int color)
        {
            drawRectangle(pixels, windowWidth, windowHeight, (int)(position.X * cellSize.X) - size / 2, (int)(position.Y * cellSize.Y - size / 2), size, size, color);
        }

        void drawRectangle(int[] img, int width, int height, int x, int y, int w, int h, int color)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var cx = x + i;
                    var cy = y + j;
                    img[cx + cy * width] = color;
                }
            }
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            if (updated)
            {
                render();
                updateFrame();
            }
            target.Draw(sprite, states);
            updated = false;
        }

        protected override void OnUpdate(float time)
        {
            var rotationSpeed = 3.1415f / 180 * 5f;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                playerDirection -= rotationSpeed;
                updated = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                playerDirection += rotationSpeed;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                playerPosition.X += 0.1f * (float)Math.Cos(playerDirection);
                playerPosition.Y += 0.1f * (float)Math.Sin(playerDirection);
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerPosition.X -= 0.1f * (float)Math.Cos(playerDirection);
                playerPosition.Y -= 0.1f * (float)Math.Sin(playerDirection);
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                fov++;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                fov--;
                updated = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
                Game.Instance.Window.Close();

        }

        int pack_color(byte r, byte g, byte b, byte a = 255)
        {
            return (a << 24) + (b << 16) + (g << 8) + r;
        }

        Tuple<byte, byte, byte, byte> unpack_color(int color)
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
