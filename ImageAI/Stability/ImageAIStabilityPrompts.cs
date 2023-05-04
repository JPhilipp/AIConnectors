using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ImageAIStabilityPrompts
{
    [JsonProperty("text")]
    public string text { get; set; }

    [JsonProperty("weight")]
    public float weight { get; set; }

    public ImageAIStabilityPrompts(string text, float weight = 1f)
    {
        this.text = text;
        this.weight = weight;
    }

    public static string ToJson(List<ImageAIStabilityPrompts> prompts)
    {
        return JsonConvert.SerializeObject(prompts, Formatting.Indented);
    }

    public static List<ImageAIStabilityPrompts> FromJson(string json)
    {
        return JsonConvert.DeserializeObject<List<ImageAIStabilityPrompts>>(json);
    }
}
