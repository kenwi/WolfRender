using System;
using System.Drawing;
using WolfRender;

public class TextureLoader
{
    public int[] BlueStone { get; private set; }
    public int[] GreyStone { get; private set; }
    public int[] Wood { get; private set; }
    public int[] ColorStone { get; private set; }


    public TextureLoader()
    {
        BlueStone = LoadTexture(@"Textures//bluestone.png");
        ColorStone = LoadTexture(@"Textures//colorstone.png");
        GreyStone = LoadTexture(@"Textures//greystone.png");
        Wood = LoadTexture(@"Textures//wood.png");
    }

    private int[] LoadTexture(string filePath)
    {
        using Bitmap bitmap = new Bitmap(filePath);
        int width = bitmap.Width;
        int height = bitmap.Height;
        var pixels = new int[width * height];

        // Load pixel values into the BlueStone array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                pixels[x + y * width] = Tools.PackColor(pixelColor.R, pixelColor.G, pixelColor.B);
            }
        }
        return pixels;
    }
} 