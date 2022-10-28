using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ImageAIParamsReplicate
{
    // Parameters when prompting Replicate's Stable Diffusion model Stability-AI.
    // https://replicate.com/stability-ai/stable-diffusion


    // The text prompt to create the image, e.g. "person climbing a mountain, lowpoly"
    public string prompt = null;

    // Width and height of output image. "Maximum size is 1024x768 or 768x1024 because of memory limits"
    // Use mutliples of 128.
    public int width = 512;
    public int height = 512;

    // "Number of images to output"
    [JsonProperty("num_outputs")]
    public int numOutputs = 1;

    // "Number of denoising steps"
    [JsonProperty("num_inference_steps")]
    public int steps = 50;

    // "Prompt strength when using init image. 1.0 corresponds to full destruction of information
    // in init image"
    [JsonProperty("prompt_strength")]
    public float promptStrength = 0.8f;

    // "Scale for classifier-free guidance"
    [JsonProperty("guidance_scale")]
    public float guidanceScale = 7.5f;

    // "Random seed. Leave blank to randomize the seed"
    public int? seed = null;

    // "Inital image to generate variations of. Will be resized to the specified width and height"
    [JsonProperty("init_image")]
    public string initImage = null;

    // "Black and white image to use as mask for inpainting over init_image. Black pixels are
    // inpainted and white pixels are preserved. Experimental feature, tends to work better
    // with prompt strength of 0.5-0.7"
    public string mask = null;

}
