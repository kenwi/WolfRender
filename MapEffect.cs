using System;
using SFML.System;
using SFML.Window;
using SFML.Graphics;

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
        int mapSizeX;
        int mapSizeY;
        int windowWidth => (int)Game.Instance.Window.Size.X;
        int windowHeight => (int)Game.Instance.Window.Size.Y;

        int rayNum = 20;
        float rayStep = 0.1f;
        float fov;

        Vector2f cellSize;
        Texture texture;
        Sprite sprite;

        float degToRad(float radians) => radians * 0.01745329f;

        public MapEffect() : base("MapTestEffect")
        {
            fov = MathF.PI * 0.5f;
            mapSizeX = 16;
            mapSizeY = 16;
            texture = new Texture(Game.Instance.Window.Size.X, Game.Instance.Window.Size.Y);
            cellSize = new Vector2f(windowWidth / mapSizeX, windowHeight / mapSizeY);
            pixels = new int[windowWidth * windowHeight];
            Game.Instance.Player.Position = new Vector2f(mapSizeX / 4, mapSizeY / 4);

            render();
            updateFrame();
        }

        void render()
        {
            if (Game.Instance.Player == null)
                return;
                
            renderMap();
            render3D();
        }
        
        private void renderMap()
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
        }

        private void render3D()
        {
            int rect_w = windowWidth / mapSizeX;
            int rect_h = windowHeight / mapSizeY;
            for (int i = 0; i < windowWidth; i++)
            {
                var playerDirection = Game.Instance.Player.Direction;
                var angle = playerDirection - fov * 0.5f + fov * i / windowWidth;
                for (float rayLength = 0; rayLength < rayNum; rayLength += rayStep)
                {
                    var playerPosition = Game.Instance.Player.Position;
                    int cx = (int)(playerPosition.X + rayLength * MathF.Cos(angle));
                    int cy = (int)(playerPosition.Y + rayLength * MathF.Sin(angle));

                    int pix_x = cx * rect_w;
                    int pix_y = cy * rect_h;

                    if (map[cx + cy * mapSizeX] != 0)
                    {
                        var dist = rayLength * Math.Cos(angle - playerDirection);
                        var columnHeight = Math.Min(2000, windowHeight / dist);
                        // var columnHeight = windowHeight / (rayLength * Math.Cos(angle - playerDirection));
                        var color = pack_color((byte)(255 - Math.Clamp(rayLength * rayLength, 10, 255)), 0, 0);
                        drawRectangle(pixels, windowWidth, windowHeight, i, (int)(windowHeight / 2 - columnHeight / 2), 1, (int)columnHeight, color);
                        break;
                    }
                }
            }
        }

        float raycast(Vector2f position, float direction, bool render = false)
        {
            float length = 0;
            for (; length < rayNum; length += rayStep)
            {
                var dx = position.X + length * Math.Cos(direction);
                var dy = position.Y + length * Math.Sin(direction);

                if (render)
                    setPixel(new Vector2f((float)dx, (float)dy), pack_color(0, 255, 0));

                if (map[(int)dx + (int)dy * mapSizeX] != 0)
                    break;

            }
            return length;
        }

        void updateFrame()
        {
            var bytes = new byte[windowWidth * windowHeight * 4];
            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            texture.Update(bytes);
        }

        void setPixel(Vector2f position, int color)
        {
            int x = (int)(position.X * cellSize.X);
            int y = (int)(position.Y * cellSize.Y);

            int index = x + y * windowWidth;
            pixels[index] = color;
        }

        void drawEntity(Vector2f position, int size, int color)
        {
            drawRectangle(pixels, windowWidth, windowHeight, (int)(position.X * cellSize.X) - size / 2, (int)(position.Y * cellSize.Y - size / 2), size, size, color);
        }

        void drawRectangle(int[] img, int width, int height, int x, int y, int w, int h, int color)
        {
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

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            render();
            updateFrame();
            sprite = new Sprite(texture);
            target.Draw(sprite, states);
        }

        protected override void OnUpdate(float dt)
        {
            var playerPosition = Game.Instance.Player.Position;
            var playerDirection = Game.Instance.Player.Direction;
            var rotationSpeed = 3.1415f / 180 * 100;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Left))
            {
                playerDirection -= rotationSpeed * dt;
            }

            if (Keyboard.IsKeyPressed(Keyboard.Key.Right))
            {
                playerDirection += rotationSpeed * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
            {
                playerPosition.X += 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y += 1 * (float)Math.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
            {
                playerPosition.X -= 1 * (float)Math.Cos(playerDirection) * dt;
                playerPosition.Y -= 1 * (float)Math.Sin(playerDirection) * dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageUp))
            {
                fov += dt;
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.PageDown))
            {
                fov -= dt;
            }
            Game.Instance.Player.Position = playerPosition;
            Game.Instance.Player.Direction = playerDirection;

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
