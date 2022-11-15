using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class TextAIHelper
{
    public const string newline = "\r\n";
    const string newlineEscaped  = "\\r\\n";
    public const string listBullet = "-";
    
    public static async void GenerateObjectsFromSamples<T>(TextAI textAI, List<T> samples, System.Action<List<T>> callback, int maxAmount = 100, bool showWarnings = false, bool useCache = true, float temperature = 0.7f, float presencePenalty = 0f) where T : class
    {
        if (!(samples?.Count >= 1)) { Debug.LogError("Missing samples"); return; }

        var serializerSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore  
        };

        List<string> jsonSamples = samples.ConvertAll(
            e => JsonConvert.SerializeObject(e, Formatting.None, serializerSettings));
        for (int i = 0; i < jsonSamples.Count; i++)
        {
            int number = i < jsonSamples.Count - 1 ? i + 1 : maxAmount;
            jsonSamples[i] = "/* " + number + " */ " + jsonSamples[i];
        }
        
        string lastJsonSample = jsonSamples[jsonSamples.Count - 1];
        jsonSamples.RemoveAt(jsonSamples.Count - 1);

        string prompt = string.Join(newline, jsonSamples);
        string suffix = lastJsonSample;

        // Debug.Log(prompt); Debug.Log(suffix);
        // return;

        List<T> items = new List<T>();
        
        string result = await textAI.GetCompletion(prompt,
            useCache: useCache, cacheKey: "generated_" + jsonSamples[0], maxTokens: 3500,
            temperature: temperature, suffix: suffix, presencePenalty: presencePenalty
        );
        if (result != null)
        {
            List<string> jsonResults = new List<string>(
                result.Split(new string[] {newline}, System.StringSplitOptions.RemoveEmptyEntries)
            );

            int max = Mathf.Min(maxAmount, jsonResults.Count);
            foreach (string jsonResult in jsonResults)
            {
                try
                {
                    T item = JsonConvert.DeserializeObject<T>(jsonResult);
                    items.Add(item);
                }
                catch (System.Exception exception)
                {
                    if (showWarnings)
                    {
                        Debug.LogWarning("Not JSON: " + jsonResult);
                        Debug.LogWarning(exception);
                    }
                }
                if (items.Count >= max) { break; }
            }
        }

        callback?.Invoke(items);
    }

    public static async void Generate<T>(TextAI textAI, List<T> samples, System.Action<List<T>> callback, int maxAmount = 100, bool showWarnings = false) where T : class
    {
        var serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        
        List<string> jsonSamples = samples.ConvertAll(
            e => JsonConvert.SerializeObject(e, Formatting.None, serializerSettings)
        );
        string prompt = TextAIHelper.GetListPrompt(
            caption: "Here are " + maxAmount + " JSON lines:",
            startItems: jsonSamples
        );

        List<T> items = new List<T>();

        string result = await textAI.GetCompletion(
            prompt, useCache: true, cacheKey: "listgrow", maxTokens: 1000, temperature: 0f);
        if (result != null)
        {
            List<string> jsonResults = TextAIHelper.GetListItemsFromResult(result);

            int max = Mathf.Min(maxAmount, jsonResults.Count);
            for (int i = 0; i < max; i++)
            {
                string jsonResult = jsonResults[i];

                T item = null;
                try
                {
                    item = JsonConvert.DeserializeObject<T>(jsonResult);
                    items.Add(item);
                }
                catch (System.Exception exception)
                {
                    if (showWarnings)
                    {
                        Debug.LogWarning("Error loading new item: ");
                        Debug.LogWarning(jsonResult);
                        Debug.LogWarning(exception);
                    }
                }
            }
        }

        callback?.Invoke(items);
    }

    public static string RemoveResultNewlines(string s)
    {
        s = s.Replace("\\n\\n", " ");
        s = s.Replace("\\n", " ");
        s = s.Replace("  ", "");
        s = s.Trim();
        return s;
    }

    public static string GetListPrompt(string caption = null, List<string> startItems = null, bool lastItemIsPartial = false)
    {
        string s = "";

        if (!string.IsNullOrEmpty(caption))
        {
            if (!caption.Contains(":")) { caption += ":"; }
            s += caption + newline;
        }

        if (startItems != null)
        {
            int count = startItems.Count;
            for (int i = 0; i < count; i++)
            {
                s += listBullet + " " + startItems[i];
                if (!(lastItemIsPartial && i == count - 1))
                {
                    s += newline;
                }
            }
        }
        
        if (!lastItemIsPartial) { s += listBullet; }

        return s;
    }

    public static List<string> GetListItemsFromResult(string s, bool doLog = false)
    {
        List<string> items = new List<string>();

        if (!string.IsNullOrEmpty(s))
        {
            string[] parts = parts = s.Split(newline);
            // if (doLog) { Debug.Log("List length: " + parts.Length); }

            for (var i = 0; i < parts.Length; i++)
            {
                string item = parts[i];
                if (i == 0 || item.Contains(listBullet))
                {
                    item = item.Replace(listBullet, "");
                    item = item.Trim();
                    item = Grammar.RemoveAorAnFromStart(item);
                    
                    if (doLog) { Debug.Log(item); }
                    items.Add(item);
                }
                else
                {
                    break;
                }
            }
        }

        return items;
    }
}
