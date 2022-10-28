using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ImageAIReplicate : MonoBehaviour
{
    // Uses a Stable Diffusion model run on the Replicate cloud.
    // https://replicate.com/stability-ai/stable-diffusion
    // The alternative class ImageAI runs locally and is free.

    public static string key = null;

    const int callCountMaxForSecurity = 10;
    static int callCount = 0;
    const float repeatDelay = 1f;

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, int steps = 50, float promptStrength = 0.8f, int? seed = null, byte[] initImage = null, byte[] mask = null, string cacheKey = null)
    {
        ImageAIParamsReplicate aiParams = new ImageAIParamsReplicate()
        {
            prompt = prompt,
            width = width,
            height = height,
            steps = steps,
            promptStrength = promptStrength,
            seed = seed,
            initImage = ImageBytesToDataString(initImage),
            mask = ImageBytesToDataString(mask)
        };
        return GetImage(callback, aiParams, useCache, cacheKey);
    }

    public IEnumerator GetImage(System.Action<Texture2D> callback, ImageAIParamsReplicate aiParams, bool useCache = false, string cacheKey = null)
    {
        Cache cache = new Cache("ImageAI", "png");

        byte[] cacheContent = null;
        if (useCache)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = aiParams.width + "x" + aiParams.height + "_" + aiParams.prompt;
                if (aiParams.seed != null) { cacheKey = cacheKey + "_" + aiParams.seed; }
            }

            cacheKey = Cache.ToKey(cacheKey);
            cacheContent = cache.GetData(cacheKey);
            if (cacheContent != null)
            {
                callback?.Invoke(GetTextureFromData(cacheContent));
            }
        }

        if (cacheContent == null)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("ImageAI Replicate.com key not set.");
                yield return null;
            }
            else if (callCount < callCountMaxForSecurity)
            {
                callCount++;

                const string apiUrl = "https://api.replicate.com/v1/predictions";
                UnityWebRequest www = UnityWebRequest.Post(apiUrl, "");
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Token " + key);

                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                var replicateParams = new ImageAIParamsWrapperReplicate {input = aiParams};
                string jsonString = JsonConvert.SerializeObject(replicateParams, Formatting.None, serializerSettings);;
                Debug.Log(jsonString);

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
                    Debug.Log("StableDiffusionImageAI result: " + result);

                    var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;
                    string progressUrl = jsonData.SelectToken("urls.get").ToString();
                    if (!string.IsNullOrEmpty(progressUrl))
                    {
                        www.Dispose();
                        yield return new WaitForSeconds(repeatDelay);

                        StartCoroutine(
                            CheckCompletionProgress(progressUrl, (byte[] data) =>
                            {
                                if (useCache) { cache.SetData(cacheKey, data); }
                                callback?.Invoke(GetTextureFromData(data));
                            }
                        ));
                    }
                    else
                    {
                        www.Dispose();
                        Debug.LogWarning("Couldn't find urls.get in ImageAI Json");
                        yield return null;
                    }
                }
            }
            else
            {
                Debug.LogWarning("ImageAI Call count limit reached.");
                yield return null;
            }
        }
    }

    Texture2D GetTextureFromData(byte[] data)
    {
        Texture2D texture = new Texture2D(0, 0);
        ImageConversion.LoadImage(texture, data);
        return texture;
    }

    public IEnumerator CheckCompletionProgress(string url, System.Action<byte[]> callback)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("Authorization", "Token " + key);
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
            string completedAt = jsonData.SelectToken("completed_at")?.ToString();
            if (string.IsNullOrEmpty(completedAt))
            {
                // Debug.Log("...");
                www.Dispose();
                yield return new WaitForSeconds(repeatDelay);
                StartCoroutine(CheckCompletionProgress(url, callback));
            }
            else if (jsonData.SelectToken("status").ToString() == "succeeded")
            {
                string imageUrl = jsonData.SelectToken("output[0]").ToString();
                www.Dispose();
                StartCoroutine(
                    DownloadImage(imageUrl, (byte[] data) =>
                    {
                        callback?.Invoke(data);
                    }
                ));
            }
            else
            {
                Debug.LogWarning("ImageAI generation failed. Result: " + result);
                www.Dispose();
            }
        }
    }

    public IEnumerator DownloadImage(string imageUrl, System.Action<byte[]> callback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning(www.error);
            www.Dispose();
        }
        else
        {
            byte[] data = www.downloadHandler.data;
            www.Dispose();
            callback?.Invoke(data);
        }
    }

    static string ImageBytesToDataString(byte[] bytes)
    {
        return bytes != null ?
            "data:image/png;base64," + System.Convert.ToBase64String(bytes) : null;
    }
}