using System.Collections.Generic;
using UnityEngine;

public static class ImageFloodFill
{
    // Successively fills an image based on one or multiple start points,
    // with optional support for a color-difference-allowed threshold.
    // Filling from the sides with a transparent black can also be useful
    // for removing the background of a center-object, AI-generated image.
    // The result can then optionally be bottom-aligned.

    public static void FillFromPoint(Texture2D texture, Color color, Vector2Int point, float threshold = 0f, float contour = 0f, bool removeLeftoverDither = false, bool bottomAlignImage = false)
    {
        var points = new Vector2Int[] { point };
        FillFromPoints(texture, color, points, threshold, contour, removeLeftoverDither, bottomAlignImage);
    }

    public static void FillFromSides(Texture2D texture, Color color, float threshold = 0f, float contour = 0f, bool removeLeftoverDither = false, bool bottomAlignImage = false)
    {
        var points = new Vector2Int[]
        {
            new Vector2Int(0,                 0),
            new Vector2Int(texture.width - 1, 0),
            new Vector2Int(0,                 texture.height - 1),
            new Vector2Int(texture.width - 1, texture.height - 1),

            new Vector2Int(0,                 texture.height / 2),
            new Vector2Int(texture.width - 1, texture.height / 2),

            new Vector2Int(texture.width / 2, 0),
            new Vector2Int(texture.width / 2, texture.height - 1),
        };
        FillFromPoints(texture, color, points, threshold, contour, removeLeftoverDither, bottomAlignImage);
    }

    public static void FillFromPoints(Texture2D texture, Color color, Vector2Int[] points, float threshold = 0f, float contour = 0f, bool removeLeftoverDither = false, bool bottomAlignImage = false)
    {
        Color[,] pixelsGrid = GetPixelsGrid(texture);
       
        foreach (Vector2Int point in points)
        {
            FillPixels(pixelsGrid, point, color, threshold);
        }

        if (removeLeftoverDither)
        {
            RemoveLeftoverDither(pixelsGrid, color);
        }

        if (bottomAlignImage)
        {
            BottomAlignImage(pixelsGrid, color);
        }

        if (contour > 0f)
        {
            AddContour(pixelsGrid, color, contour);
        }

        texture.SetPixels(GetPixelsLinearFromGrid(pixelsGrid));
        texture.Apply();
    }

    static void FillPixels(Color[,] pixels, Vector2Int startPoint, Color color, float threshold)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        var size = new RectInt(0, 0, width, height);
        bool[,] pixelsHandled = new bool[width, height];
        Color originColor = pixels[startPoint.x, startPoint.y];

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

    static void BottomAlignImage(Color[,] pixels, Color fillColor)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        var rect = new RectInt(0, 0, width, height);

        int bottomY = GetBottomRow(pixels, fillColor);
        if (bottomY != -1)
        {
            int moveDownAmount = height - bottomY;
            for (int y = height - 1; y >= moveDownAmount; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x, y] = pixels[x, y - moveDownAmount];
                }
            }
            for (int y = 0; y < moveDownAmount; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x, y] = fillColor;
                }
            }
        }
    }

    static int GetBottomRow(Color[,] pixels, Color fillColor)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[x, y] != fillColor)
                {
                    return y;
                }
            }
        }

        return -1;
    }

    static void RemoveLeftoverDither(Color[,] pixels, Color fillColor)
    {
        const int blockSize = 3;
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        const int blockPixelAmountUponWhichToClear = 2;
        
        for (int x = 0; x < width; x += blockSize)
        {
            for (int y = 0; y < height; y += blockSize)
            {
                var rect = new RectInt(x, y, blockSize, blockSize);
                int amount = GetFrontPixelsAmount(pixels, rect);
                if (amount >= 1 && amount <= blockPixelAmountUponWhichToClear)
                {
                    ClearPixels(pixels, rect, fillColor);
                }
            }
        }
    }

    static int GetFrontPixelsAmount(Color[,] pixels, RectInt rect)
    {
        int amount = 0;
        RectInt size = new RectInt(0, 0, pixels.GetLength(0), pixels.GetLength(1));

        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                if (size.Contains(new Vector2Int(x, y)) && pixels[x, y].a > 0f)
                {
                    amount++;
                }
            }
        }

        return amount;
    }

    static void ClearPixels(Color[,] pixels, RectInt rect, Color fillColor)
    {
        RectInt size = new RectInt(0, 0, pixels.GetLength(0), pixels.GetLength(1));

        for (int x = rect.xMin; x < rect.xMax; x++)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                if (size.Contains(new Vector2Int(x, y)))
                {
                    pixels[x, y] = fillColor;
                }
            }
        }
    }

    static void AddContour(Color[,] pixels, Color backColor, float contour)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        var size = new RectInt(0, 0, width, height);
        Color contourColor = new Color(backColor.r, backColor.g, backColor.b, 1f);

        Color[,] contourPixels = new Color[width, height];
        
        int maxOffset = (int) Mathf.Ceil(contour);
        float maxDistance = Vector2.Distance(
            new Vector2Int(0, 0), new Vector2Int(maxOffset, maxOffset));

        ClearEdges(pixels, backColor, edgeSize: maxOffset + 1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pixels[x, y].a == 0f)
                {
                    (float? distance, Color frontColor) = DistanceToNearestFrontColor(
                        pixels, new Vector2Int(x, y), backColor, size,
                        maxOffset: maxOffset);
                    if (distance != null)
                    {
                        float alpha = Mathf.Clamp01((maxDistance - (float) distance) / contour);
                        
                        Color mixColor = Color.Lerp(
                            new Color(frontColor.r, frontColor.g, frontColor.b, alpha),
                            new Color(contourColor.r, contourColor.g, contourColor.b, alpha),
                            0.5f);
                        contourPixels[x, y] = mixColor;
                    }
                }
            }
        }

        StampPixels(pixels, contourPixels);
    }

    static void ClearEdges(Color[,] pixels, Color backColor, int edgeSize)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        var innerSize = new RectInt(
            edgeSize, edgeSize, width - edgeSize * 2, height - edgeSize * 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!innerSize.Contains(new Vector2Int(x, y)))
                {
                    pixels[x, y] = backColor;
                }
            }
        }
    }

    static void StampPixels(Color[,] pixels, Color[,] stamp)
    {
        int width  = pixels.GetLength(0);
        int height = pixels.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (stamp[x, y].a > 0f)
                {
                    pixels[x, y] = stamp[x, y];
                }
            }
        }
    }

    static (float? distance, Color frontColor) DistanceToNearestFrontColor(Color[,] pixels, Vector2Int point, Color backColor, RectInt size, int maxOffset)
    {
        Color frontColor = Color.clear;
        float? minDistance = null;

        for (int xOffset = -maxOffset; xOffset <= maxOffset; xOffset++)
        {
            for (int yOffset = -maxOffset; yOffset <= maxOffset; yOffset++)
            {
                Vector2Int offsetPoint = point + new Vector2Int(xOffset, yOffset);
                if (size.Contains(offsetPoint) && pixels[offsetPoint.x, offsetPoint.y].a == 1f)
                {
                    float distance = Vector2.Distance(point, offsetPoint);
                    if (minDistance == null || distance < minDistance)
                    {
                        minDistance = distance;
                        frontColor = pixels[offsetPoint.x, offsetPoint.y];
                    }
                }
            }
        }
        return (minDistance, frontColor);
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
                pixels[x, height - y - 1] = pixelsLinear[y * width + x];
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
                pixelsLinear[y * width + x] = pixelsGrid[x, height - y - 1];
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
