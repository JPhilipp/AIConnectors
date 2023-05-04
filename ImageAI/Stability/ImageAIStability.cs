using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class ImageAIStability : MonoBehaviour
{
    // Converts text-to-image via the paid Stability.ai/ Dreamstudio API.
    // https://platform.stability.ai/rest-api#tag/v1generation
    // The alternative class ImageAI runs locally and is free,
    // but Dreamstudio may allow newer Stable Diffusion versions.

    public static string key = null;
    const int callCountMaxForSecurity = 25;
    static int callCount = 0;

    [Header("Info")]
    [SerializeField] string resultingCachePath = "";

    // Call ShowAvailableEngines() to get a list of all engineIDs.
    public string engineId = "stable-diffusion-xl-beta-v2-2-2";

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, byte[] image = null, byte[] mask = null, string cacheKey = null)
    {
        ImageAIParamsStability aiParams = new ImageAIParamsStability()
        {
            prompts = new List<ImageAIStabilityPrompts>(){new ImageAIStabilityPrompts(prompt)},
            width = width,
            height = height
        };
        return GetImage(callback, aiParams, useCache, cacheKey);
    }

    public IEnumerator GetImage(System.Action<Texture2D> callback, ImageAIParamsStability aiParams, bool useCache = false, string cacheKey = null)
    {
        Cache cache = new Cache("ImageAIStability", "png");

        byte[] cacheContent = null;
        if (useCache)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = aiParams.width + "x" + aiParams.height + "_" + aiParams.prompts[0].text;
            }

            cacheKey = Cache.ToKey(cacheKey, allowSlash: true);
            resultingCachePath = cache.GetPathByKey(cacheKey);

            while (cache.IsReserved(cacheKey))
            {
                yield return new WaitForSeconds(0.1f);
            }

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
                Debug.LogError("ImageAIStability key not set.");
                yield return null;
            }
            else if (callCount < callCountMaxForSecurity)
            {
                cache.Reserve(cacheKey);
                callCount++;
                Debug.Log("callCount = " + callCount);

                string apiUrl = "https://api.stability.ai/v1/generation/" + engineId + "/text-to-image";
                string jsonString = null;

                var serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                jsonString = JsonConvert.SerializeObject(aiParams, Formatting.None, serializerSettings);
 
                using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, ""))
                {
                    www.SetRequestHeader("Authorization", "Bearer " + key);
                    www.SetRequestHeader("Content-Type", "application/json");
                    www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));

                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogWarning(www.error);
                    }
                    else
                    {
                        string result = www.downloadHandler.text;

                        var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;

                        var jsonToken = jsonData.SelectToken("artifacts[0]['base64']");
                        string base64Image = jsonToken != null ? jsonToken.ToString() : null;
                        if (!string.IsNullOrEmpty(base64Image))
                        {
                            byte[] data = System.Convert.FromBase64String(base64Image);
                            if (useCache) { cache.SetData(cacheKey, data, createKeyFoldersIfNeeded: true); }
                            callback?.Invoke(ImageAIHelper.GetTextureFromData(data));
                        }
                        else
                        {
                            var jsonErrorToken = jsonData.SelectToken("['error']['message']");
                            string errorMessage = jsonErrorToken != null ? jsonErrorToken.ToString() : null;
                            Debug.LogWarning("Couldn't find image in ImageAIStability Json: " + errorMessage +
                                "\r\n" + result);
                        }
                    }
                    cache.ReleaseReservation(cacheKey);
                    yield return null;
                }
            }
            else
            {
                Debug.LogWarning("ImageAIStability call count limit reached.");
                yield return null;
            }
        }
    }

    public IEnumerator ShowAvailableEngines()
    {
        Debug.Log("Getting available Stability.ai engines...");
        string apiUrl = "https://api.stability.ai/v1/engines/list";
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            www.SetRequestHeader("Authorization", "Bearer " + key);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.ConnectionError)
            {
                string result = www.downloadHandler.text;
                Debug.Log("The following Stability.ai engines are available:");
                var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JArray;
                foreach (var engine in jsonData)
                {
                    Debug.Log("    " + engine["id"].ToString() + " (" + engine["name"].ToString() + "): " +
                        engine["description"].ToString());
                }
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }   
}