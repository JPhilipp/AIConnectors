using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
namespace Universe
{

[DisallowMultipleComponent] public class UniverseStart : MonoBehaviour
{
    // The base class to launch everything, like the generation of the universe.
    // Here we show how a cube is textured via StableDiffusion, and a text completed
    // via GPT-3.

    [SerializeField] GameObject testCube = null;

    UniverseGenerator universeGenerator = null;
    ImageAI imageAI = null;
    TextAI textAI = null;

    void Awake()
    {
        const string pathPrefix = @"D:\_misc\";
        ImageAI.key = System.IO.File.ReadAllText(pathPrefix + "replicate-key.txt");
        TextAI.key  = System.IO.File.ReadAllText(pathPrefix + "openai-key.txt");
        CoroutineVariant.TextAI.key  = System.IO.File.ReadAllText(pathPrefix + "openai-key.txt");

        textAI = GetComponent<TextAI>();
    }

    async void Start()
    {
        // TestTextAI();
        // TestImageAI();
        // StartCoroutine(TestWhenAll_Coroutine());
    }

    async void TestTextAI()
    {
        const string prompt = "Albert Einstein was";
        
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        string result = await textAI.GetCompletion(prompt, useCache: false, temperature: 0f, showResultInfo: false, maxTokens: 200);
        stopwatch.Stop();
        Debug.Log($"TestTextAI elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log(result);
    }

    void TestTextAI_Coroutine()
    {
        CoroutineVariant.TextAI textAICoroutine = Misc.GetAddComponent<CoroutineVariant.TextAI>(gameObject);
        const string prompt = "Albert Einstein was";

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        StartCoroutine(
            textAICoroutine.GetCompletion(prompt, (string result) =>
            {
                stopwatch.Stop();
                Debug.Log($"TestTextAI elapsed time: {stopwatch.ElapsedMilliseconds} ms");
                Debug.Log(result);
            },
            useCache: false, temperature: 0f, showResultInfo: false, maxTokens: 200
        ));
    }

    async void TestWhenAll()
    {
        Debug.Log("TestWhenAll");

        Task<string> a = textAI.GetCompletion("Albert Einstein was", useCache: false);
        Task<string> b = textAI.GetCompletion("Susan Sarandon is",   useCache: false);
        
        await Task.WhenAll(a, b);
        
        Debug.Log("a: " + a.Result);
        Debug.Log("b: " + b.Result);
    }

    IEnumerator TestWhenAll_Coroutine()
    {
        Debug.Log("TestWhenAll_Coroutine");
        CoroutineVariant.TextAI textAICoroutine = Misc.GetAddComponent<CoroutineVariant.TextAI>(gameObject);

        string a = null;
        string b = null;
        StartCoroutine(textAICoroutine.GetCompletion("Albert Einstein was", (result) => a = result));
        StartCoroutine(textAICoroutine.GetCompletion("Susan Sarandon is",   (result) => b = result));
        yield return new WaitUntil(()=>
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b);
        });

        Debug.Log("a: " + a);
        Debug.Log("b: " + b);
    }

    void TestImageAI()
    {
        imageAI = GetComponent<ImageAI>();

        string prompt = "xmas tree";
        const string promptSuffix = ", minimalist 3d";
        prompt += promptSuffix;
        Debug.Log("Sending prompt " + prompt);

        StartCoroutine(
            imageAI.GetImage(prompt, (Texture2D texture) =>
            {
                Debug.Log("Done.");
                Renderer renderer = testCube.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
            },
            useCache: false,
            width: 256, height: 256
        ));
    }
}

}