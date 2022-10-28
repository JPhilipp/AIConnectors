using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class TextAI : MonoBehaviour
{
    // Grabs GPT-3 completions via the OpenAI API, with optional result caching.
    // CoroutineVariant.TextAI can be used as alternative to async-await.

    public static string key = null;

    const int callCountMaxForSecurity = 150;
    static int callCount = 0;

    public Task<string> GetCompletion(string prompt, bool useCache = false, string cacheKey = null, float secondsDelay = 0f, int maxTokens = 100, string[] stop = null, float temperature = 0.7f, float presencePenalty = 0f, float frequencyPenalty = 0f, string suffix = null, bool showResultInfo = false, int responseLengthMaxGoal = 0, string model = TextAIParams.defaultModel)
    {
        TextAIParams aiParams = new TextAIParams()
        {
            prompt = prompt,
            maxTokens = maxTokens,
            temperature = temperature,
            presencePenalty = presencePenalty,
            frequencyPenalty = frequencyPenalty,
            stop = stop,
            suffix = suffix,
            model = model
        };

        if (responseLengthMaxGoal > 0)
        {
            aiParams.maxTokens = GetApproximateTokensNeededForResponseLength(prompt, responseLengthMaxGoal);
        }

        return GetCompletion(aiParams, useCache, cacheKey, secondsDelay, showResultInfo);
    }

    public async Task<string> GetCompletion(TextAIParams aiParams, bool useCache = false, string cacheKey = null, float secondsDelay = 0f, bool showResultInfo = false)
    {
        Cache cache = new Cache("TextAI", "json");

        string cacheContent = null;
        if (useCache)
        {
            if (cacheKey == null) { cacheKey = aiParams.prompt; }
            cacheKey = Cache.ToKey(cacheKey);
            cacheContent = cache.GetText(cacheKey);
            if (!string.IsNullOrEmpty(cacheContent))
            {
                string result = GetResultFromJsonString(cacheContent,
                    showResultInfo: showResultInfo, cacheWasUsed: true);
                return result;
            }
        }

        if (string.IsNullOrEmpty(cacheContent))
        {
            aiParams.CapMaxTokensToAllowed();

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("TextAI OpenAI.com key not set.");
                return null;
            }            
            else if (callCount < callCountMaxForSecurity)
            {
                callCount++;

                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                string jsonString = JsonConvert.SerializeObject(aiParams, Formatting.None, serializerSettings);
                // Debug.Log(jsonString);

                string apiUrl = "https://api.openai.com/v1/completions";
                UnityWebRequest www = UnityWebRequest.Post(apiUrl, "");
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + key);
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
                www.downloadHandler = new DownloadHandlerBuffer();

                await www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    www.Dispose();
                    return null;
                }
                else
                {
                    string text = www.downloadHandler.text;
                    string result = GetResultFromJsonString(text, showResultInfo: showResultInfo);
                    if (useCache) { cache.SetText(cacheKey, text); }

                    www.Dispose();

                    return result;
                }
            }
            else
            {
                Debug.Log("OpenAI Call count limit reached.");
                return null;
            }
        }

        return null;
    }

    string GetResultFromJsonString(string jsonString, bool showResultInfo = false, bool cacheWasUsed = false)
    {
        string result = null;

        var jsonData = JsonConvert.DeserializeObject(jsonString) as Newtonsoft.Json.Linq.JObject;
        try
        {        
            result = jsonData.SelectToken("choices[0].text").ToString();

            if (showResultInfo)
            {
                Debug.Log("- Finish reason: " + jsonData.SelectToken("choices[0].finish_reason").ToString());
                int totalTokens = jsonData.SelectToken("usage.total_tokens").ToObject<int>();

                // Price is here and may change: https://openai.com/api/pricing/
                const float daVinciCostPerThousandInUsd = 0.02f;
                float usd = daVinciCostPerThousandInUsd * totalTokens * 0.001f;

                string info = "- Total tokens used: " + totalTokens + ", " +
                    "$" + usd;
                if (cacheWasUsed) { info += " (but $0 as retrieved from cache)"; }
                Debug.Log(info);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(exception.Message);
            Debug.Log(jsonString);
        }

        return result;
    }

    public static int GetApproximateTokensNeededForResponseLength(string prompt, int responseLengthGoal)
    {
        const int approximateTokenToCharRatio = 4;
        return (prompt.Length + responseLengthGoal) / approximateTokenToCharRatio;
    }
}
