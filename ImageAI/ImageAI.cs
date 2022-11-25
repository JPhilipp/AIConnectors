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

    [Header("Info")]
    [SerializeField] string resultingCachePath = "";

    public const string cacheFolder = "ImageAI";

    public IEnumerator GetImage(string prompt, System.Action<Texture2D> callback, bool useCache = false, int width = 512, int height = 512, int steps = 50, int promptStrength = 7, int seed = -1, byte[] image = null, byte[] mask = null, string cacheKey = null, bool tiling = false, float denoisingStrength = 0.75f, string negativePrompt = null)
    {
        ImageToImageAIParams aiParams = new ImageToImageAIParams()
        {
            prompt = prompt,
            width = width,
            height = height,
            seed = seed,
            promptStrength = promptStrength,
            steps = steps,
            tiling = tiling,
            negativePrompt = negativePrompt,

            initImages = image != null ? new string[] { ImageAIHelper.ImageBytesToDataString(image) } : null,
            mask = ImageAIHelper.ImageBytesToDataString(mask),
            denoisingStrength = image != null ? denoisingStrength : 0f
        };
        return GetImage(callback, aiParams, useCache, cacheKey);
    }

    public IEnumerator GetImage(System.Action<Texture2D> callback, ImageToImageAIParams aiParams, bool useCache = false, string cacheKey = null)
    {
        Cache cache = new Cache(cacheFolder, "png");

        byte[] cacheContent = null;
        if (useCache)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = aiParams.width + "x" + aiParams.height + "_" + aiParams.prompt;
                if (aiParams.seed != -1) { cacheKey = cacheKey + "_" + aiParams.seed; }
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
            cache.Reserve(cacheKey);

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

            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning(www.error);
                www.Dispose();
                yield return null;
            }
            else
            {
                string result = www.downloadHandler.text;

                try
                {
                    var jsonData = JsonConvert.DeserializeObject(result) as Newtonsoft.Json.Linq.JObject;
                    string base64Image = jsonData.SelectToken("images[0]").ToString();
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
                        Debug.LogWarning("Couldn't find image in ImageAI Json");
                    }
                }
                catch (System.Exception exception)
                {
                    www.Dispose();
                    Debug.LogWarning("Couldn't parse ImageAI Json: " + exception.Message + ". Result: " + result);
                }
                cache.ReleaseReservation(cacheKey);
                yield return null;
            }
        }
    }
}