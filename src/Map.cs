using System;
using System.IO;
using SFML.System;
using SFML.Graphics;

namespace WolfRender
{
    public class Map
    {
        public int[,] Data { get; private set; }
        public Texture MapTexture { get; private set; }
        public Vector2i Size { get; private set; }

        public Map()
        {
            LoadFromBitmap("map.bmp"); // Default constructor can load a default map
        }

        public void LoadFromBitmap(string filename)
        {
            try
            {
                // Load the image using SFML
                Image mapImage = new Image(filename);
                Size = new Vector2i((int)mapImage.Size.X, (int)mapImage.Size.Y);
                Data = new int[Size.X, Size.Y];
                MapTexture = new Texture(mapImage); 
                
                // Convert each pixel to map data
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int x = 0; x < Size.X; x++)
                    {
                        Color pixel = mapImage.GetPixel((uint)x, (uint)y);
                        
                        // Convert pixel to grayscale and threshold
                        float brightness = (pixel.R + pixel.G + pixel.B ) / (3.0f * 255.0f);
                        
                        // If pixel is darker than 50% gray, it's a wall
                        Data[x, y] = brightness < 0.5f ? 1 : 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading map from bitmap: {e.Message}");
                // Fall back to a simple default map if loading fails
                CreateDefaultMap();
            }
        }

        private void CreateDefaultMap()
        {
            // Create a simple default map if loading fails
            Size = new Vector2i(20, 20);
            Data = new int[Size.X, Size.Y];
            
            // Fill the borders with walls
            for (int x = 0; x < Size.X; x++)
            {
                Data[x, 0] = 1;
                Data[x, Size.Y - 1] = 1;
            }
            for (int y = 0; y < Size.Y; y++)
            {
                Data[0, y] = 1;
                Data[Size.X - 1, y] = 1;
            }
        }

        public int Get(Vector2i position)
        {
            if (position.X < 0 || position.X >= Size.X || 
                position.Y < 0 || position.Y >= Size.Y)
                return 1; // Return wall for out of bounds

            return Data[position.X, position.Y];
        }
    }
}
