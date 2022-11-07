using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ImageAIDallE : MonoBehaviour
{
    // Converts text-to-image via the paid OpenAI Dall-E 2 API.
    // https://beta.openai.com/docs/guides/images/introduction
    // Pricing: https://openai.com/api/pricing/
    // The alternative class ImageAI runs locally and is free.

    public static string key = null;
    const int callCountMaxForSecurity = 10;
    static int callCount = 0;

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, byte[] image = null, byte[] mask = null, string cacheKey = null)
    {
        ImageAIParamsDallE aiParams = new ImageAIParamsDallE()
        {
            prompt = prompt,
            width = width,
            height = height

            // Image and Mask not yet supported.
            // image = ImageAIHelper.ImageBytesToDataString(image),
            // mask = ImageAIHelper.ImageBytesToDataString(mask)
        };
        return GetImage(callback, aiParams, useCache, cacheKey);
    }

    public IEnumerator GetImage(System.Action<Texture2D> callback, ImageAIParamsDallE aiParams, bool useCache = false, string cacheKey = null)
    {
        Cache cache = new Cache("ImageAI", "png");

        byte[] cacheContent = null;
        if (useCache)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = aiParams.width + "x" + aiParams.height + "_" + aiParams.prompt;
            }

            cacheKey = Cache.ToKey(cacheKey, allowSlash: true);
            cacheContent = cache.GetData(cacheKey);
            if (cacheContent != null)
            {
                callback?.Invoke(ImageAIHelper.GetTextureFromData(cacheContent));
            }
        }

        if (cacheContent == null)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("ImageAIDallE OpenAI key not set.");
                yield return null;
            }
            else if (!aiParams.IsValidSize())
            {
                Debug.LogError("ImageAIDallE used with none of the allowed square sizes " +
                    string.Join(", ", ImageAIParamsDallE.allowedSizes));
                yield return null;
            }
            else if (callCount < callCountMaxForSecurity)
            {
                callCount++;

                string apiMode = "generations";
                if (aiParams.image != null)
                {
                    apiMode = aiParams.mask != null ? "edits" : "variations";
                }
                string apiUrl = "https://api.openai.com/v1/images/" + apiMode;

                UnityWebRequest www = UnityWebRequest.Post(apiUrl, "");
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + key);

                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                string jsonString = JsonConvert.SerializeObject(aiParams, Formatting.None, serializerSettings);

                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
                www.downloadHandler = new DownloadHandlerBuffer();

                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogWarning(www.error);
                    www.Dispose();
                }
                else
                {
                    string result = www.downloadHandler.text;

                    var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;
                    string base64Image = jsonData.SelectToken("data[0]['" + aiParams.responseFormat + "']").ToString();
                    if (!string.IsNullOrEmpty(base64Image))
                    {
                        byte[] data = System.Convert.FromBase64String(base64Image);
                        www.Dispose();
                        if (useCache) { cache.SetData(cacheKey, data, createKeyFoldersIfNeeded: true); }
                        callback?.Invoke(ImageAIHelper.GetTextureFromData(data));
                    }
                    else
                    {
                        www.Dispose();
                        Debug.LogWarning("Couldn't find image in ImageAIDallE Json");
                        yield return null;
                    }
                }
            }
            else
            {
                Debug.LogWarning("ImageAIDallE Call count limit reached.");
                yield return null;
            }
        }
    }
}