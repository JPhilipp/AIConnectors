using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
namespace Universe
{

[DisallowMultipleComponent] public class UniverseStart : MonoBehaviour
{
    // Here we show how a cube is textured via StableDiffusion, and a
    // text completed via GPT-3.

    [SerializeField] GameObject testCube = null;
    
    TextAI textAI = null;

    void Awake()
    {
        const string pathPrefix = @"D:\_misc\";

        Cache.rootFolder = pathPrefix + "Cache";

        string openAIKey = File.ReadAllText(pathPrefix + "openai-key.txt");
        TextAI.key = openAIKey;
        CoroutineVariant.TextAI.key = openAIKey;
        ImageAIDallE.key = openAIKey;

        ImageAIStability.key = File.ReadAllText(pathPrefix + "stability-key.txt");
        ImageAIReplicate.key = File.ReadAllText(pathPrefix + "replicate-key.txt");

        textAI = Misc.GetAddComponent<TextAI>(gameObject);
    }

    void Start()
    {
        // TestTextAI();
        // TestTextAI_GPT3();
        // TestTextAI_Coroutine();
        // TestImageAI();
        // TestImageAIStability();
        // StartCoroutine(TestWhenAll_Coroutine());
    }

    async void TestTextAI()
    {
        const string prompt = "Who was Albert Einstein? Thanks!";
        
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        string result = await textAI.GetAnswer(prompt, useCache: false, temperature: 0f, showResultInfo: false, maxTokens: 200);
        stopwatch.Stop();
        Debug.Log($"TestTextAI elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log(result);
    }

    async void TestTextAI_GPT3()
    {
        const string prompt = "Albert Einstein was";
        
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        string result = await textAI.GetAnswer(prompt, useCache: false, temperature: 0f, showResultInfo: false, maxTokens: 200,
            model: "text-davinci-003", endpoint: "/v1/completions");
        stopwatch.Stop();
        Debug.Log($"TestTextAI elapsed time: {stopwatch.ElapsedMilliseconds} ms");
        Debug.Log(result);
    }

    void TestTextAI_Coroutine()
    {
        CoroutineVariant.TextAI textAICoroutine = Misc.GetAddComponent<CoroutineVariant.TextAI>(gameObject);
        const string prompt = "Who was Albert Einstein? Thanks!";

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        StartCoroutine(
            textAICoroutine.GetAnswer(prompt, (string result) =>
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

        Task<string> a = textAI.GetAnswer("Who was Albert Einstein?", useCache: false);
        Task<string> b = textAI.GetAnswer("Who is Susan Sarandon?",   useCache: false);
        
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
        StartCoroutine(textAICoroutine.GetAnswer("Who was Albert Einstein?", (result) => a = result));
        StartCoroutine(textAICoroutine.GetAnswer("Who is Susan Sarandon?",   (result) => b = result));
        yield return new WaitUntil(()=>
        {
            return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b);
        });

        Debug.Log("a: " + a);
        Debug.Log("b: " + b);
    }

    void TestImageAI()
    {
        ImageAI imageAI = Misc.GetAddComponent<ImageAI>(gameObject);

        string prompt = "person on mountain, minimalist 3d";
        Debug.Log("Sending prompt " + prompt);

        StartCoroutine(
            imageAI.GetImage(prompt, (Texture2D texture) =>
            {
                Debug.Log("Done.");
                Renderer renderer = testCube.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
            },
            useCache: false,
            width: 512, height: 512
        ));
    }

    void TestImageAIStability()
    {
        ImageAIStability imageAI = Misc.GetAddComponent<ImageAIStability>(gameObject);

        // StartCoroutine(imageAI.ShowAvailableEngines());

        string prompt = "person on mountain, minimalist 3d";
        Debug.Log("Sending prompt " + prompt);

        StartCoroutine(
            imageAI.GetImage(prompt, (Texture2D texture) =>
            {
                Debug.Log("Done.");
                Renderer renderer = testCube.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
            },
            useCache: false,
            width: 512, height: 512
        ));
    }

    void TestImageAIDallE()
    {
        ImageAIDallE imageAI = Misc.GetAddComponent<ImageAIDallE>(gameObject);

        string prompt = "wizard's hut, black background, artstation asset";
        Debug.Log("Sending prompt " + prompt);

        const int size = 1024;

        StartCoroutine(
            imageAI.GetImage(prompt, (Texture2D texture) =>
            {
                Debug.Log("Done.");
                Renderer renderer = testCube.GetComponent<Renderer>();
                
                Color fillColor = new Color(0f, 0f, 0.2f, 0f);
                ImageFloodFill.FillFromSides(
                    texture, fillColor,
                    threshold: 0.075f, contour: 5f, bottomAlignImage: true);
                
                renderer.material.mainTexture = texture;
            },
            useCache: true,
            width: size, height: size
        ));
    }
}

}
