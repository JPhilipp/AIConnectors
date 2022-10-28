using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ImageAI : MonoBehaviour
{
    // Uses a Stable Diffusion model run locally via Automatic1111's API.
    // https://sphuff.dev/automatic-now-has-an-api
    // http://localhost:7860/docs#/default/text2imgapi_sdapi_v1_txt2img_post
    // The alternative class ImageAIReplicate connects to the cloud and is paid.

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, int steps = 50, int promptStrength = 7, int seed = -1, byte[] initImage = null, byte[] mask = null, string cacheKey = null)
    {
        ImageToImageAIParams aiParams = new ImageToImageAIParams()
        {
            prompt = prompt,
            width = width,
            height = height,
            seed = seed,
            promptStrength = promptStrength,
            steps = steps,

            initImages = initImage != null ? new string[] { ImageBytesToDataString(initImage) } : null,
            mask = ImageBytesToDataString(mask)
        };
        return GetImage(callback, aiParams, useCache, cacheKey);
    }

    public IEnumerator GetImage(System.Action<Texture2D> callback, ImageToImageAIParams aiParams, bool useCache = false, string cacheKey = null)
    {
        Cache cache = new Cache("ImageAI", "png");

        byte[] cacheContent = null;
        if (useCache)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = aiParams.width + "x" + aiParams.height + "_" + aiParams.prompt;
                if (aiParams.seed != -1) { cacheKey = cacheKey + "_" + aiParams.seed; }
            }

            cacheKey = Cache.ToKey(cacheKey, allowSlash: true);
            cacheContent = cache.GetData(cacheKey);
            if (cacheContent != null)
            {
                callback?.Invoke(GetTextureFromData(cacheContent));
            }
        }

        if (cacheContent == null)
        {
            string apiMode = aiParams.initImages != null ? "img2img" : "txt2img";
            string apiUrl = "http://localhost:7860/sdapi/v1/" + apiMode;

            UnityWebRequest www = UnityWebRequest.Post(apiUrl, "");
            www.SetRequestHeader("Content-Type", "application/json");

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            string jsonString = JsonConvert.SerializeObject(aiParams, Formatting.None, serializerSettings);
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
                // Debug.Log("ImageAI result: " + result);

                var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;
                string base64Image = jsonData.SelectToken("images[0]").ToString();
                if (!string.IsNullOrEmpty(base64Image))
                {
                    byte[] data = System.Convert.FromBase64String(base64Image);
                    www.Dispose();
                    if (useCache) { cache.SetData(cacheKey, data, createKeyFoldersIfNeeded: true); }
                    callback?.Invoke(GetTextureFromData(data));
                }
                else
                {
                    www.Dispose();
                    Debug.LogWarning("Couldn't find image in ImageAI Json");
                    yield return null;
                }
            }
        }
    }

    Texture2D GetTextureFromData(byte[] data)
    {
        Texture2D texture = new Texture2D(0, 0);
        ImageConversion.LoadImage(texture, data);
        return texture;
    }

    static string ImageBytesToDataString(byte[] bytes)
    {
        return bytes != null ?
            "data:image/png;base64," + System.Convert.ToBase64String(bytes) : null;
    }
}