using System.Collections.Generic;
using UnityEngine;

public static class ImageFloodFill
{
    // Successively fills an image based on one or multiple start points,
    // with optional support for a color-difference-allowed threshold.
    // Filling from corners with a transparent black can also be useful
    // for removing the background of a center-object, AI-generated image.

    public static void FillFromPoint(Texture2D texture, Color color, Vector2Int point, float threshold = 0f)
    {
        var points = new Vector2Int[] { point };
        FillFromPoints(texture, color, points, threshold);
    }

    public static void FillFromCorners(Texture2D texture, Color color, float threshold = 0f)
    {
        var points = new Vector2Int[]
        {
            new Vector2Int(0,                 0),
            new Vector2Int(texture.width - 1, 0),
            new Vector2Int(0,                 texture.height - 1),
            new Vector2Int(texture.width - 1, texture.height - 1)
        };
        FillFromPoints(texture, color, points, threshold);
    }

    public static void FillFromPoints(Texture2D texture, Color color, Vector2Int[] points, float threshold = 0f)
    {
        Color[,] pixelsGrid = GetPixelsGrid(texture);
       
        foreach (Vector2Int point in points)
        {
            FillPixels(pixelsGrid, point, color, threshold);
        }

        texture.SetPixels(GetPixelsLinearFromGrid(pixelsGrid));
        texture.Apply();
    }

    static void FillPixels(Color[,] pixels, Vector2Int startPoint, Color color, float threshold)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        bool[,] pixelsHandled = new bool[width, height];
        Color originColor = pixels[startPoint.x, startPoint.y];
        var size = new RectInt(0, 0, width, height);

        var stack = new Stack<Vector2Int>();
        stack.Push(startPoint);

        while (stack.Count > 0)
        {
            Vector2Int point = stack.Pop();
            if (size.Contains(point) && !pixelsHandled[point.x, point.y])
            {
                pixelsHandled[point.x, point.y] = true;
                if (ColorDistance(pixels[point.x, point.y], originColor) <= threshold)
                {
                    pixels[point.x, point.y] = color;

                    stack.Push(new Vector2Int(point.x - 1, point.y));
                    stack.Push(new Vector2Int(point.x + 1, point.y));
                    stack.Push(new Vector2Int(point.x, point.y - 1));
                    stack.Push(new Vector2Int(point.x, point.y + 1));
                }
            }
        }
    }

    static Color[,] GetPixelsGrid(Texture2D texture)
    {
        int width  = texture.width;
        int height = texture.height;
        Color[] pixelsLinear = texture.GetPixels();
        Color[,] pixels = new Color[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixels[x, y] = pixelsLinear[y * height + x];
            }
        }

        return pixels;
    }

    static Color[] GetPixelsLinearFromGrid(Color[,] pixelsGrid)
    {
        int width  = pixelsGrid.GetLength(0);
        int height = pixelsGrid.GetLength(1);
        Color[] pixelsLinear = new Color[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                pixelsLinear[y * height + x] = pixelsGrid[x, y];
            }
        }

        return pixelsLinear;
    }

    static float ColorDistance(Color color1, Color color2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(color1.r - color2.r, 2) +
            Mathf.Pow(color1.g - color2.g, 2) +
            Mathf.Pow(color1.b - color2.b, 2)
        );
    }
}
