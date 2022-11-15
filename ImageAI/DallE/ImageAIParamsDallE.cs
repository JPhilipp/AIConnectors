using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageAIParamsDallE
{
    // Parameters for the Dall-E 2 text-to-image API.
    // https://beta.openai.com/docs/api-reference/images

    // The text prompt to create the image, e.g. "person climbing a mountain, lowpoly"
    public string prompt = null;

    public readonly static int[] allowedSizes = { 256, 512, 1024 };

    // Can use the square-only sizes listed of allowedSizes.
    public string size { get { return width + "x" + height; } }

    [JsonIgnore]
    public int width  = 512;

    [JsonIgnore]
    public int height = 512;

    // "The format in which the generated images are returned. Must be one of url or b64_json.
    // Defaults to url"
    [JsonProperty("response_format")]
    public string responseFormat = "b64_json";

    // "A unique identifier representing your end-user, which will help OpenAI to
    // monitor and detect abuse. ... End-user IDs are essential to our safety approach,
    // and by default, if you are serving end-users you must send us end-user IDs."
    [JsonProperty("user")]
    public string userId = null;


    [JsonProperty("n")]
    public int amount = 1;


    // "Must be a valid PNG file, less than 4MB, and square."
    // When no mask is used, this is the image to use as the basis for the variations.
    // When mask is used too, this is the image to edit.
    
    public byte[] image = null;

    // "An additional image whose fully transparent areas (e.g. where alpha is zero) indicate
    // where image should be edited. Must be a valid PNG file, less than 4MB, and have the
    // same dimensions as image."
    [JsonIgnore]
    public byte[] mask = null;


    public bool IsValidSize()
    {
        return width == height && allowedSizes.Contains(width);
    }
}
