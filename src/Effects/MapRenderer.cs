using System;
using SFML.System;
using SFML.Graphics;
using System.Threading.Tasks;

namespace WolfRender
{
    public class MapRenderer : Effect
    {
        Map map;
        Player player;
        Vector2u cellSize;
        Vector2u textureSize;
        Texture texture;

        public float Fov { get; set; } = (float)Math.PI / 3.0f;
        public Map Map { get => map; }

        int[] pixels;
        byte[] bytes;
        Sprite sprite;
        uint windowWidth, windowHeight;
        double lightMultiplier = 1.0f;

        readonly int[] colorPalette = {
            Tools.PackColor(0, 0, 0),         // 0: Black
            Tools.PackColor(140, 28, 28),     // 1: Dark blood red
            Tools.PackColor(74, 92, 37),      // 2: Military green
            Tools.PackColor(47, 54, 82),      // 3: Dark navy blue
            Tools.PackColor(165, 98, 10),     // 4: Rusty orange
            Tools.PackColor(128, 128, 128),   // 5: Medium gray
            Tools.PackColor(48, 48, 48),      // 6: Dark gray
            Tools.PackColor(89, 47, 23),      // 7: Dark brown
            Tools.PackColor(163, 136, 89),    // 8: Tan/Beige
            Tools.PackColor(66, 28, 28),      // 9: Darker blood red
        };

        // Update wall texture to use new color indices for a more DOOM-like look
        readonly int[] wallTexture = {
            1, 9, 1, 9, 9, 1, 9, 1,  // Blood red pattern
            9, 7, 8, 7, 7, 8, 7, 9,  // Brown and tan pattern
            1, 8, 7, 1, 1, 7, 8, 1,
            9, 7, 1, 7, 7, 1, 7, 9,
            9, 7, 1, 7, 7, 1, 7, 9,
            1, 8, 7, 1, 1, 7, 8, 1,
            9, 7, 8, 7, 7, 8, 7, 9,
            1, 9, 1, 9, 9, 1, 9, 1,
        };

        readonly int[] floorTexture = {
            7, 6, 7, 6, 7, 6, 7, 6,  // Dark brown and gray checkerboard
            6, 8, 6, 8, 6, 8, 6, 8,  // With some tan highlights
            7, 6, 7, 6, 7, 6, 7, 6,
            6, 8, 6, 8, 6, 8, 6, 8,
            7, 6, 7, 6, 7, 6, 7, 6,
            6, 8, 6, 8, 6, 8, 6, 8,
            7, 6, 7, 6, 7, 6, 7, 6,
            6, 8, 6, 8, 6, 8, 6, 8,
        };

        readonly int[] ceilingTexture = {
            6, 6, 6, 6, 6, 6, 6, 6,  // Mostly dark gray
            6, 3, 6, 6, 6, 6, 3, 6,  // With some dark blue highlights
            6, 6, 6, 6, 6, 6, 6, 6,
            6, 6, 6, 3, 3, 6, 6, 6,
            6, 6, 6, 3, 3, 6, 6, 6,
            6, 6, 6, 6, 6, 6, 6, 6,
            6, 3, 6, 6, 6, 6, 3, 6,
            6, 6, 6, 6, 6, 6, 6, 6,
        };

        public MapRenderer(Map map, Player player) : base("MapRenderer")
        {
            this.map = map;
            this.player = player;
            windowWidth = Instance.Window.Size.X;
            windowHeight = Instance.Window.Size.Y;
            
            var view = Instance.Window.GetView();
            windowWidth = (uint)view.Size.X;
            windowHeight = (uint)view.Size.Y;

            texture = new Texture(windowWidth, windowHeight);
            cellSize = new Vector2u(windowWidth / map.Size.X, windowHeight / map.Size.Y);
            textureSize = new Vector2u(8, 8); // Since all textures are 8x8
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

        int GetTextureValue(int x, int y, int idx, uint size, int[] textureArray) 
            => textureArray[x + idx * size + y * size];

        // Replace the GetFloorTexCoord method with this perspective-correct version
        (int, int) GetFloorTexCoord(int x, int y, double angle)
        {
            // Use the absolute angle directly - no need to adjust for player direction since it's already included
            double rayDirX = Math.Cos(angle);
            double rayDirY = Math.Sin(angle);

            // Position of the camera plane relative to screen height
            int screenHeight = (int)windowHeight;
            double cameraHeight = screenHeight / 2.0;

            // Current y position compared to the center of the screen (the horizon)
            double currentDist = screenHeight / (2.0 * y - screenHeight);

            // Calculate the real world coordinates of the point on the floor
            double floorX = player.Position.X + currentDist * rayDirX;
            double floorY = player.Position.Y + currentDist * rayDirY;

            // Get the texture coordinates
            int texX = (int)(floorX * textureSize.X) % (int)textureSize.X;
            int texY = (int)(floorY * textureSize.Y) % (int)textureSize.Y;

            if (texX < 0) texX += (int)textureSize.X;
            if (texY < 0) texY += (int)textureSize.Y;

            return (texX, texY);
        }

        // Modify the CalculateShade method to better handle different distance ranges
        private float CalculateShade(double distance)
        {
            // Adjust these values to fine-tune the shading
            const float maxDistance = 16.0f;  // Increased for better floor visibility
            const float minShade = 0.1f;     // Darker minimum
            
            // Add exponential falloff for more dramatic distance shading
            float shade = (float)Math.Pow(1.0f - (distance / maxDistance), 2.0);
            return Math.Max(minShade, shade);
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            pixels = new int[windowWidth * windowHeight];
            var depthBuffer = new float[windowWidth];

            // Precalculate constants
            double fovHalf = Fov * 0.5;
            int halfHeight = (int)windowHeight / 2;
            int textureMask = (int)textureSize.X - 1;

            // Use Parallel.For for the main ray casting loop
            Parallel.For(0, (int)windowWidth, x =>  // Explicit cast to int
            {
                double angle = player.Direction - fovHalf + (x * Fov) / windowWidth;
                
                // Precalculate ray direction
                double rayDirX = Math.Cos(angle);
                double rayDirY = Math.Sin(angle);

                // Current map position
                int mapX = (int)player.Position.X;
                int mapY = (int)player.Position.Y;

                // Length of ray from one x or y-side to next x or y-side
                double deltaDistX = Math.Abs(1 / rayDirX) * 100;
                double deltaDistY = Math.Abs(1 / rayDirY) * 100;

                // Calculate step and initial sideDist
                int stepX = rayDirX < 0 ? -1 : 1;
                int stepY = rayDirY < 0 ? -1 : 1;
                
                double sideDistX = rayDirX < 0 
                    ? (player.Position.X - mapX) * deltaDistX 
                    : (mapX + 1.0 - player.Position.X) * deltaDistX;
                
                double sideDistY = rayDirY < 0 
                    ? (player.Position.Y - mapY) * deltaDistY 
                    : (mapY + 1.0 - player.Position.Y) * deltaDistY;

                // Perform DDA
                bool hit = false;
                int side = 0;
                while (!hit)
                {
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    if (map.Get(new Vector2i(mapX, mapY)) > 0)
                        hit = true;
                }

                // Calculate perpendicular distance
                double perpWallDist = side == 0
                    ? (mapX - player.Position.X + (1 - stepX) / 2) / rayDirX
                    : (mapY - player.Position.Y + (1 - stepY) / 2) / rayDirY;

                // Calculate wall height and drawing bounds
                int lineHeight = (int)(windowHeight / perpWallDist);
                int drawStart = Math.Max(0, -lineHeight / 2 + halfHeight);
                int drawEnd = Math.Min((int)windowHeight - 1, lineHeight / 2 + halfHeight);

                // Calculate wall texture coordinates
                double wallX = side == 0
                    ? player.Position.Y + perpWallDist * rayDirY
                    : player.Position.X + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                int wallTexX = (int)(wallX * textureSize.X) & textureMask;
                if ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0))
                    wallTexX = ((int)textureSize.X - 1) - wallTexX;

                // Precalculate texture step and position
                double step = textureSize.Y / (double)lineHeight;
                double texPos = (drawStart - halfHeight + lineHeight / 2.0) * step;

                // Calculate wall shading using perpendicular distance
                float wallShade = CalculateShade(perpWallDist);

                // Draw walls with shading
                int xOffset = x;
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int wallTexY = (int)texPos & textureMask;
                    texPos += step;

                    int colorIdx = GetTextureValue(wallTexX, wallTexY, 0, (uint)textureSize.X, wallTexture);
                    int baseColor = colorPalette[colorIdx];

                    // Apply wall shading
                    pixels[xOffset + y * (int)windowWidth] = Tools.PackColor(
                        (byte)((baseColor >> 16 & 0xFF) * wallShade),
                        (byte)((baseColor >> 8 & 0xFF) * wallShade),
                        (byte)((baseColor & 0xFF) * wallShade)
                    );
                }

                // Draw floor and ceiling with distance-based shading
                for (int y = 0; y < drawStart; y++)
                {
                    // Calculate real distance to the floor/ceiling point
                    double currentDist = Math.Abs(windowHeight / (2.0 * y - windowHeight)); // Ensure it's positive
                    
                    // Scale the distance to match wall distance scale
                    double scaledDist = currentDist * Math.Abs(Math.Cos(angle - player.Direction));
                    
                    float ceilingShade = CalculateShade(scaledDist);
                    float floorShade = ceilingShade; // Use the same shade for both

                    // Ceiling
                    var (floorTexX, floorTexY) = GetFloorTexCoord(x, y, angle + Math.PI);
                    int colorIdx = GetTextureValue(floorTexX, floorTexY, 0, (uint)textureSize.X, ceilingTexture);
                    int baseColor = colorPalette[colorIdx];
                    
                    // Apply ceiling shading
                    pixels[xOffset + y * (int)windowWidth] = Tools.PackColor(
                        (byte)((baseColor >> 16 & 0xFF) * ceilingShade),
                        (byte)((baseColor >> 8 & 0xFF) * ceilingShade),
                        (byte)((baseColor & 0xFF) * ceilingShade)
                    );

                    // Floor
                    int floorY = (int)windowHeight - y - 1;
                    (floorTexX, floorTexY) = GetFloorTexCoord(x, floorY, angle);
                    colorIdx = GetTextureValue(floorTexX, floorTexY, 0, (uint)textureSize.X, floorTexture);
                    baseColor = colorPalette[colorIdx];
                    
                    // Apply floor shading
                    pixels[xOffset + floorY * (int)windowWidth] = Tools.PackColor(
                        (byte)((baseColor >> 16 & 0xFF) * floorShade),
                        (byte)((baseColor >> 8 & 0xFF) * floorShade),
                        (byte)((baseColor & 0xFF) * floorShade)
                    );
                }
            });

            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            texture.Update(bytes);
            target.Draw(sprite, states);
        }

        protected override void OnUpdate(float time)
        {
            lightMultiplier = Math.Abs(Math.Sin(Instance.TotalGameTime.AsSeconds()));
        }
    }
}
