using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageAIParams
{
    // Parameters for the local Automatic1111 StableDiffusion API.

    // The text prompt to create the image, e.g. "person climbing a mountain, lowpoly"
    public string prompt = null;

    public int width = 512;

    public int height = 512;

    [JsonProperty("enable_hr")]
    public bool enableHighRes = false;

    [JsonProperty("firstphase_width")]
    public int firstPhaseWidth = 0;

    [JsonProperty("firstphase_height")]
    public int firstPhaseHeight = 0;

    [DefaultValueAttribute(-1)]
    public int seed = -1;

    [JsonProperty("subseed")]
    [DefaultValueAttribute(-1)]
    public int subSeed = -1;

    [JsonProperty("subseed_strength")]
    public int subSeedStrength = 0;

    [JsonProperty("seed_resize_from_h")]
    [DefaultValueAttribute(-1)]
    public int seedResizeFromHeight = -1;

    [JsonProperty("seed_resize_from_w")]
    [DefaultValueAttribute(-1)]
    public int seedResizeFromWidth = -1;

    [JsonProperty("batch_size")]
    [DefaultValueAttribute(1)]
    public int batchSize = 1;

    [JsonProperty("n_iter")]
    [DefaultValueAttribute(1)]
    public int iterations = 1;

    // "The number of denoising steps. More denoising steps usually lead to a
    // higher quality image at the expense of slower inference. This parameter
    // will be modulated by strength."
    public int steps = 50;

    [JsonProperty("cfg_scale")]
    public int promptStrength = 7;

    // string[] styles = null;

    [JsonProperty("restore_faces")]
    public bool restoreFaces = false;

    public bool tiling = false;

    // "The prompt or prompts not to guide the image generation. Ignored when not using
    // guidance (i.e., ignored if guidance_scale is less than 1)."
    [JsonProperty("negative_prompt")]
    public string negativePrompt = null;

    // "The special case of η = 0 makes the sampling process deterministic"
    public int eta = 0;

    [JsonProperty("s_churn")]
    public int stochasticityChurn = 0;

    [JsonProperty("s_tmax")]
    public int stochasticityTmax = 0;

    [JsonProperty("s_tmin")]
    public int stochasticityTmin = 0;

    [JsonProperty("s_noise")]
    [DefaultValueAttribute(1)]
    public int stochasticityNoise = 1;

    // "Euler a: Euler Ancestral - very creative, each can get a completely different picture
    // depending on step count, setting steps to higher than 30-40 does not help
    // DDIM": Denoising Diffusion Implicit Models - best at inpainting"
    [JsonProperty("sampler_index")]
    public string samplingMethodString { get { return GetSamplingMethodString(); } }

    [JsonIgnore]
    public ImageAISamplingMethod samplingMethod = ImageAISamplingMethod.EulerAncestral;

    public string GetSamplingMethodString()
    {
        var methodStrings = new Dictionary<ImageAISamplingMethod, string>()
        {
            { ImageAISamplingMethod.EulerAncestral, "Euler a" }, // Default in the Web UI
            { ImageAISamplingMethod.Euler, "Euler" }, // Apparent Default in the API
            { ImageAISamplingMethod.LMS, "LMS" },
            { ImageAISamplingMethod.Heun, "Heun" },
            { ImageAISamplingMethod.DPM2, "DPM2" },
            { ImageAISamplingMethod.DPM2Ancestral, "DPM2 a" },
            { ImageAISamplingMethod.DPMFast, "DPM fast" },
            { ImageAISamplingMethod.DPMAdaptive, "DPM adaptive" },
            { ImageAISamplingMethod.LMSKarras, "LMS Karras" },
            { ImageAISamplingMethod.DPM2Karras, "DPM2 Karras" },
            { ImageAISamplingMethod.DPM2AncestralKarras, "DPM2 a Karras" },
            { ImageAISamplingMethod.DDIM, "DDIM" },
            { ImageAISamplingMethod.PLMS, "PLMS" }
        };
        return methodStrings[samplingMethod];
    }
}
