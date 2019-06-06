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
            1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1,
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
        // int MapSize => (int)Math.Sqrt(map.Length);
        int mapSizeX;
        int mapSizeY;
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
            mapSizeX = 16;
            mapSizeY = 16;
            cellSize = new Vector2(windowWidth / mapSizeX, windowHeight / mapSizeY);
            pixels = new int[windowWidth * windowHeight];

            fov = 80f * 0.01745329f;
            playerDirection = 0;
            playerPosition = new Vector2(mapSizeX / 4, mapSizeY / 4);

            updated = true;
            render();
            updateFrame();
        }

        void render()
        {
            int cellId = 0;
            for (int y = 0; y < mapSizeY; y++)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    var cellColorId = map[cellId++];
                    var cellColor = palette.AsSpan(cellColorId * 3, 3);
                    drawRectangle(pixels, windowWidth, windowHeight, x * (int)cellSize.X, y * (int)cellSize.Y, (int)cellSize.X, (int)cellSize.Y, pack_color(cellColor[0], cellColor[1], cellColor[2]));
                }
            }
            drawEntity(playerPosition, 5, pack_color(0, 0, 255));

            for (int i = 0; i < windowWidth; i++)
            {
                var angle = playerDirection - (fov / 2) + fov * i / windowWidth;
                bool visible = (i == 0 || i == windowWidth - 1) ? true : false;
                if (visible)
                    raycast(playerPosition, angle, visible);

                for (float rayLength = 0; rayLength < 20; rayLength += 0.02f)
                {
                    float cx = playerPosition.X + rayLength * MathF.Cos(angle);
                    float cy = playerPosition.Y + rayLength * MathF.Sin(angle);

                    int rect_w = windowWidth / mapSizeX;
                    int rect_h = windowHeight / mapSizeY;

                    int pix_x = (int)cx * rect_w;
                    int pix_y = (int)cy * rect_h;

                    if (map[(int)cx + (int)cy * mapSizeX] != 0)
                    {
                        var columnHeight = windowHeight / (rayLength * Math.Cos(angle - playerDirection));
                        drawRectangle(pixels, windowWidth, windowHeight, i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight, pack_color(255, 0, 0));
                        break;
                    }
                }
            }
            // for (int i = 0; i < windowWidth; i++)
            // {
            //     bool visible = (i == 0 || i == windowWidth - 1) ? true : false;
            //     var angle = playerDirection - (fov / 2) + fov * i / windowWidth;
            //     var length = raycast(playerPosition, angle, visible);
            //     var columnHeight = windowHeight / (length * Math.Cos(angle - playerDirection));
            //     // drawRectangle(pixels, windowWidth, windowHeight, i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight-1, pack_color(255, 0, 0));
            // }

        }

        float raycast(Vector2 position, float direction, bool render = false)
        {
            float length = 0, step = 0.01f;
            for (; length < 10; length += step)
            {
                var dx = position.X + length * Math.Cos(direction);
                var dy = position.Y + length * Math.Sin(direction);

                if (map[(int)dx + (int)dy * 16] != 0)
                    break;

                if (render)
                    setPixel(new Vector2((float)dx, (float)dy), pack_color(0, 255, 0));
            }
            return length;
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

            if (x < 0 || x > windowWidth)
                return;
            if (y < 0 || y > windowHeight)
                return;

            int index = x + y * windowWidth;
            pixels[index] = color;
        }

        void drawEntity(Vector2 position, int size, int color)
        {
            drawRectangle(pixels, windowWidth, windowHeight, (int)(position.X * cellSize.X) - size / 2, (int)(position.Y * cellSize.Y - size / 2), size, size, color);
        }

        void drawRectangle(int[] img, int width, int height, int x, int y, int w, int h, int color)
        {
            if (h > windowHeight)
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

        protected override void OnUpdate(float dt)
        {
            var rotationSpeed = 3.1415f / 180 * 100;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                playerDirection -= rotationSpeed * dt;
                updated = true;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                playerDirection += rotationSpeed * dt;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                playerPosition.X += 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y += 1 * (float)Math.Sin(playerDirection) * dt;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerPosition.X -= 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y -= 1 * (float)Math.Sin(playerDirection) * dt;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                fov += dt;
                updated = true;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                fov -= dt;
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
