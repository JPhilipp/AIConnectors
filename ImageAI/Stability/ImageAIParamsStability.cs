using System.ComponentModel;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ImageAIParamsStability
{
    // Parameters for the Stability.ai/ Dreamstudio text-to-image API.
    // https://platform.stability.ai/rest-api#tag/v1generation

    [JsonProperty("text_prompts")]
    public List<ImageAIStabilityPrompts> prompts = null;

    [JsonProperty("width")]
    public int width  = 512;

    [JsonProperty("height")]
    public int height = 512;

    [JsonProperty("cfg_scale")]
    public int cfgSscale = 7;

    [JsonProperty("clip_guidance_preset")]
    public string clipGuidancePreset = "NONE";

    [JsonProperty("sampler")]
    public string sampler = null;

    [JsonProperty("samples")]
    public int samples = 1;

    [JsonProperty("steps")]
    public int steps = 50;

    [JsonProperty("seed")]
    public int seed = 0;
}
