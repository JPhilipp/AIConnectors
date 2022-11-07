using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ImageAIHelper
{
    public static Texture2D GetTextureFromData(byte[] data)
    {
        Texture2D texture = new Texture2D(0, 0);
        ImageConversion.LoadImage(texture, data);
        return texture;
    }

    public static string ImageBytesToDataString(byte[] bytes)
    {
        return bytes != null ?
            "data:image/png;base64," + System.Convert.ToBase64String(bytes) : null;
    }
}
