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
        Vector2u textureSize;
        Texture screenTexture;

        public float Fov { get; set; } = (float)Math.PI / 2.0f;
        public  double FovHalf { get; set; }
        public Map Map { get => map; }
        public double ShadingExp { get; set; } = 8.0;

        int[] pixels;

        private int halfHeight;
        private int textureMask;
        byte[] bytes;
        Sprite screenSprite;
        uint windowWidth, windowHeight;
        double lightMultiplier = 1.0f;

        private TextureLoader textureLoader = new TextureLoader();
        private RectangleShape minimapBackground;
        private Sprite minimapSprite;
        private readonly CircleShape playerDot;
        private ConvexShape playerFov;
        private const float MINIMAP_SCALE = 4.0f;
        private float[] zBuffer;

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
            
            screenTexture = new Texture(windowWidth, windowHeight);
            screenSprite = new Sprite(screenTexture);
            textureSize = new Vector2u(8, 8); // Since all textures are 8x8
            bytes = new byte[windowWidth * windowHeight * 4];
            pixels = new int[windowWidth * windowHeight];

            // Precalculate constants
            FovHalf = Fov * 0.5;
            halfHeight = (int)windowHeight / 2;
            textureMask = (int)textureSize.X - 1;

            // Initialize minimap elements
            minimapBackground = new RectangleShape(new Vector2f(map.Size.X * MINIMAP_SCALE, map.Size.Y * MINIMAP_SCALE));
            minimapBackground.FillColor = new Color(0, 0, 0, 128);  // Semi-transparent black
            minimapBackground.Position = new Vector2f(windowWidth - (map.Size.X * MINIMAP_SCALE) - 10, 10);  // 10px padding

            minimapSprite = new Sprite(map.MapTexture);
            minimapSprite.Scale = new Vector2f(MINIMAP_SCALE, MINIMAP_SCALE);
            minimapSprite.Position = minimapBackground.Position;
            minimapSprite.Color = new Color(255, 255, 255, 64);  // Semi-transparent white

            playerDot = new CircleShape(2);  // 3px radius
            playerDot.FillColor = Color.Red;

            playerFov = new ConvexShape(3);  // Triangle shape
            playerFov.FillColor = new Color(0, 0, 128, 64);
            UpdateFovCone();
            CalculateZBuffer(); // Initialize the Z-buffer for floor/ceiling shading
        }

        public void UpdateFovCone()
        {
            float distanceToEdge = 20.0f;

            // Calculate the width of the triangle based on the FoV
            float width = 2 * distanceToEdge * (float)Math.Tan(Fov / 2.0f);

            // Set the points of the triangle
            playerFov.SetPoint(0, new Vector2f(0, 0)); // Apex of the triangle
            playerFov.SetPoint(1, new Vector2f(-width / 2, -distanceToEdge)); // Left point
            playerFov.SetPoint(2, new Vector2f(width / 2, -distanceToEdge)); // Right point
        }

        int GetTextureValue(int x, int y, int idx, uint size, int[] textureArray) 
            => textureArray[x + idx * size + y * size];

        (int, int) GetFloorTexCoord64x64(int x, int y, double angle)
        {
            // Use the absolute angle directly - no need to adjust for player direction since it's already included
            double rayDirX = Math.Cos(angle);
            double rayDirY = Math.Sin(angle);

            // Position of the camera plane relative to screen height
            int screenHeight = (int)windowHeight;

            // Current y position compared to the center of the screen (the horizon)
            double currentDist = screenHeight / (2.0 * y - screenHeight);

            // Calculate the real world coordinates of the point on the floor
            double floorX = player.Position.X + currentDist * rayDirX;
            double floorY = player.Position.Y + currentDist * rayDirY;

            // Get the texture coordinates
            int texX = (int)(floorX * 64) % 64;
            int texY = (int)(floorY * 64) % 64;

            if (texX < 0) texX += 64;
            if (texY < 0) texY += 64;

            return (texX, texY);
        }

        private float CalculateShade(double distance)
        {
            const float maxDistance = 64.0f;  // Increased for better floor visibility
            const float minShade = 0.1f;     // Darker minimum
            
            // Add exponential falloff for more dramatic distance shading
            float shade = (float)Math.Pow(1.0f - (distance / maxDistance), ShadingExp);
            return Math.Max(minShade, shade);
        }

        protected override void OnDraw(RenderTarget target, RenderStates states)
        {
            // Use Parallel.For for the main ray casting loop
            Parallel.For(0, (int)windowWidth, x =>
            {
                double angle = player.Direction - FovHalf + (x * Fov) / windowWidth;

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

                // Calculate texture coordinates based on the 64x64 texture size
                int wallTexX = (int)(wallX * 64) & (64 - 1); // Use 64 for texture size
                if ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0))
                    wallTexX = (64 - 1) - wallTexX;

                // Precalculate texture step and position
                double step = 64.0 / (double)lineHeight; // Use 64 for texture size
                double texPos = (drawStart - halfHeight + lineHeight / 2.0) * step;

                // Calculate wall shading using perpendicular distance
                float wallShade = CalculateShade(perpWallDist);

                // Draw walls with BlueStone texture
                int xOffset = x;
                for (int y = drawStart; y < drawEnd; y++)
                {
                    int wallTexY = (int)texPos & (64 - 1); // Use 64 for texture size
                    texPos += step;

                    // Get the color from the BlueStone texture
                    int colorIdx = wallTexX + wallTexY * 64; // Corrected index calculation
                    int wallColor = textureLoader.BlueStone[colorIdx];

                    byte r = (byte)((wallColor & 0xFF) * wallShade);
                    byte g = (byte)((wallColor >> 8 & 0xFF) * wallShade);
                    byte b = (byte)((wallColor >> 16 & 0xFF) * wallShade);
                    pixels[xOffset + y * (int)windowWidth] = Tools.PackColor(r, g, b);
                }

                // Draw floor and ceiling with distance-based shading
                int height = (int)windowHeight;
                for (int y = 0; y < drawStart; y++)
                {
                    // Lookup distance shade to the floor/ceiling from zBuffer
                    float ceilingShade = zBuffer[y];
                    float floorShade = ceilingShade;

                    // Ceiling
                    int ceilingY = height - y - 1;
                    var (ceilingTexX, ceilingTexY) = GetFloorTexCoord64x64(x, ceilingY, angle);

                    // Get the color from the GreyStone texture
                    int ceilingColorIdx = ceilingTexX + ceilingTexY * 64;
                    int ceilingColor = textureLoader.GreyStone[ceilingColorIdx];

                    // Apply ceiling shading
                    byte r = (byte)((ceilingColor & 0xFF) * ceilingShade);
                    byte g = (byte)((ceilingColor >> 8 & 0xFF) * ceilingShade);
                    byte b = (byte)((ceilingColor >> 16 & 0xFF) * ceilingShade);
                    pixels[xOffset + y * (int)windowWidth] = Tools.PackColor(r, g, b);

                    // Floor
                    int floorY = height - y - 1;
                    var (floorTexX, floorTexY) = GetFloorTexCoord64x64(x, floorY, angle);

                    // Get the color from the GreyStone texture
                    int floorColorIdx = floorTexX + floorTexY * 64;
                    int floorColor = textureLoader.GreyStone[floorColorIdx];

                    // Apply floor shading
                    r = (byte)((floorColor & 0xFF) * floorShade);
                    g = (byte)((floorColor >> 8 & 0xFF) * floorShade);
                    b = (byte)((floorColor >> 16 & 0xFF) * floorShade);
                    pixels[xOffset + floorY * (int)windowWidth] = Tools.PackColor(r, g, b);
                }
            });

            render(target, states);
        }

        public void CalculateZBuffer()
        {
            zBuffer = new float[windowHeight];

            for (int y=0; y<windowHeight; y++)
            {
                double currentDist = Math.Abs(windowHeight / (2.0 * y - windowHeight)); // Ensure it's positive
                float ceilingShade = CalculateShade(currentDist);
                zBuffer[y] = ceilingShade;
            }
        }

        private void render(RenderTarget target, RenderStates states)
        {
            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
            screenTexture.Update(bytes);
            target.Draw(screenSprite, states);
            target.Draw(minimapBackground);
            target.Draw(minimapSprite);
            target.Draw(playerFov);
            target.Draw(playerDot);
        }

        protected override void OnUpdate(float time)
        {
            lightMultiplier = Math.Abs(Math.Sin(Instance.TotalGameTime.AsSeconds()));

            // Update and draw player position on minimap
            playerFov.Position = new Vector2f(
                minimapSprite.Position.X + (player.Position.X * MINIMAP_SCALE),
                minimapSprite.Position.Y + (player.Position.Y * MINIMAP_SCALE)
            );
            playerFov.Rotation = player.Direction * 180.0f / (float)Math.PI + 90.0f;  // Convert to degrees and offset

            playerDot.Position = new Vector2f(
                minimapSprite.Position.X + (player.Position.X * MINIMAP_SCALE) - playerDot.Radius,
                minimapSprite.Position.Y + (player.Position.Y * MINIMAP_SCALE) - playerDot.Radius
            );
        }
    }
}
