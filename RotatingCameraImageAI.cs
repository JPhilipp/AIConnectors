using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
namespace Universe
{

public class RotatingCameraImageAI : MonoBehaviour
{
    // Rotates a camera around a given 3D model to generate screenshots
    // which are then converted via StableDiffusion.

    ImageAI imageAI = null;
    float rotation = 0f;
    const float rotationStep = 5f;
    const float rotationMax = 360f - rotationStep;
    bool isWorking = false;

    void Start()
    {
        imageAI = Misc.GetAddComponent<ImageAI>(gameObject);
        ApplyRotation();
        Camera.onPostRender += OnPostRenderCallback;
    }

    void ApplyRotation()
    {
        transform.SetLocalEulerAnglesY(-rotation);
    }

    void OnPostRenderCallback(Camera camera)
    {
        if (camera == Camera.main && !isWorking && rotation <= rotationMax)
        {
            isWorking = true;

            Rect region = new Rect(0, 0, Screen.width, Screen.height);
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(region, 0, 0, false);
            texture.Apply();

            byte[] imageBytes = texture.EncodeToPNG();
            ApplyRotation();

            const int seed = 1000;
            const string folder = "Greg";
            string fileName = Misc.PadWithZero(rotation, 3);
            string cacheKey = folder + "/" + fileName;

            const string prompt = "portrait of female knight, by greg manchess, bernie fuchs, ruan jia, walter everett";
            StartCoroutine(
                imageAI.GetImage(prompt, (Texture2D texture) =>
                {
                    rotation += rotationStep;
                    isWorking = false;
                },
                width: Screen.width, height: Screen.height, useCache: true, seed: seed,
                steps: 50, initImage: imageBytes, cacheKey: cacheKey
            ));
        }
    }

    void OnDestroy()
    {
        Camera.onPostRender -= OnPostRenderCallback;
    }
}

}