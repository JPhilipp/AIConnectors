using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageToImageAIParams : ImageAIParams
{
    // Extra parameters for the local Automatic1111 StableDiffusion img2img API.

    // The images used for img2img
    [JsonProperty("init_images")]
    public string[] initImages = null;

    [JsonProperty("resize_mode")]
    public int resizeMode = 0;
    
    // The mask used for img2img. Black pixels allow painting over,
    // white pixels remain.
    public string mask = null;

    // "Determines how little respect the algorithm should have for image's content. At 0,
    // nothing will change, and at 1 you'll get an unrelated image. With values below 1.0,
    // processing will take less steps than the Sampling Steps slider specifies."
    // UI default: 0.75f
    [JsonProperty("denoising_strength")]
    public float denoisingStrength = 0;
    
    [JsonProperty("mask_blur")]
    public int maskBlur = 4;
    
    [JsonProperty("inpainting_fill")]
    public int inpaintingFill = 0;
    
    [JsonProperty("inpaint_full_res")]
    public bool inpaintFullRes = true;
    
    [JsonProperty("inpaint_full_res_padding")]
    public int inpaintFullResPadding = 0;
    
    [JsonProperty("inpainting_mask_invert")]
    public int inpaintingMaskInvert = 0;
}
