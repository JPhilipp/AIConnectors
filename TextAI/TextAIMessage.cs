using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TextAIMessage
{
    [JsonProperty("role")]
    public string role { get; set; }

    [JsonProperty("content")]
    public string content { get; set; }

    public TextAIMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }

    public static string ToJson(List<TextAIMessage> messages)
    {
        return JsonConvert.SerializeObject(messages, Formatting.Indented);
    }

    public static List<TextAIMessage> FromJson(string json)
    {
        return JsonConvert.DeserializeObject<List<TextAIMessage>>(json);
    }
}
