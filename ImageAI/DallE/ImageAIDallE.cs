using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class ImageAIDallE : MonoBehaviour
{
    // Converts text-to-image via the paid OpenAI Dall-E 2 API.
    // https://beta.openai.com/docs/guides/images/introduction
    // https://beta.openai.com/docs/api-reference/images
    // Pricing: https://openai.com/api/pricing/
    // The alternative class ImageAI runs locally and is free.

    public static string key = null;
    const int callCountMaxForSecurity = 25;
    static int callCount = 0;

    const string modeGeneration = "generations";
    const string modeEdit       = "edits";
    const string modeVariation  = "variations";

    [Header("Info")]
    [SerializeField] string resultingCachePath = "";

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, byte[] image = null, byte[] mask = null, string cacheKey = null)
    {
        ImageAIParamsDallE aiParams = new ImageAIParamsDallE()
        {
            prompt = prompt,
            width = width,
            height = height,

            image = image,
            mask = mask
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
                cache.Reserve(cacheKey);
                callCount++;
                Debug.Log("callCount = " + callCount);

                string mode = modeGeneration;
                if (aiParams.image != null) { mode = aiParams.mask != null ? modeEdit : modeVariation; }
                string apiUrl = "https://api.openai.com/v1/images/" + mode;
                string jsonString = null;
                List<IMultipartFormSection> formParts = null;

                if (mode == modeGeneration)
                {
                    var serializerSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };
                    jsonString = JsonConvert.SerializeObject(aiParams, Formatting.None, serializerSettings);
                }
                else
                {
                    formParts = new List<IMultipartFormSection>();
                    
                    formParts.Add(new MultipartFormDataSection("response_format", aiParams.responseFormat));
                    formParts.Add(new MultipartFormDataSection("size", aiParams.size));
                    formParts.Add(new MultipartFormFileSection("image", aiParams.image, "image.png", "image/png"));
                    if (mode == modeEdit)
                    {
                        formParts.Add(new MultipartFormDataSection("prompt", aiParams.prompt));
                        formParts.Add(new MultipartFormFileSection("mask", aiParams.mask, "mask.png", "image/png"));
                    }
                }
 
                using (UnityWebRequest www = mode == modeGeneration ?
                    UnityWebRequest.Post(apiUrl, "") : UnityWebRequest.Post(apiUrl, formParts))
                {
                    www.SetRequestHeader("Authorization", "Bearer " + key);
                    if (mode == modeGeneration)
                    {
                        www.SetRequestHeader("Content-Type", "application/json");
                        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
                    }

                    yield return www.SendWebRequest();
                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogWarning(www.error);
                    }
                    else
                    {
                        string result = www.downloadHandler.text;

                        var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;
                        
                        var jsonToken = jsonData.SelectToken("data[0]['" + aiParams.responseFormat + "']");
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
                            Debug.LogWarning("Couldn't find image in ImageAIDallE Json: " + errorMessage +
                                "\r\n" + result);
                        }
                    }
                    cache.ReleaseReservation(cacheKey);
                    yield return null;
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