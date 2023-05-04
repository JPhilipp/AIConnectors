# AIConnectors
 **Here you'll find Unity C# connections to StableDiffusion, GPT-3, GPT-4/ ChatGPT, Stability.ai SDXL/ Dreamstudio API, Dall-E 2, and possibly other AI tools in the future.**

🡆 To see these in action, check out the [AI TV channel on YouTube](https://www.youtube.com/playlist?list=PL9LD6IUjh-8iQZ-cvfTYwnuzEDECCqrRr).

---


https://user-images.githubusercontent.com/1754503/198673075-a051cbd3-9a68-4edb-9eba-85f30abac2b0.mp4


**Stable Diffusion** is a machine learning AI that turns text (or images) into images. For instance, "knight in shiny armor, fantasy art" becomes a glorious 1024x768 painting in seconds. You'll find a class to connect Unity to a local API. Alternatively, you can also connect to the paid [Replicate.com](https://replicate.com) with your account there. Dall-E is also supported.

---

**GPT-4** is a deep learning artificial intelligence which answers text prompts. For instance, as you can try for yourself with [ChatGPT](https://chat.openai.com/), the question "Who was Albert Einstein?" gets answered with "Albert Einstein (1879-1955) was a renowned theoretical physicist and one of the greatest scientific minds in history. He was born in ..." and more. This project contains a class to connect to the OpenAI API.

---

## How to install

* **Json:** Please grab [Newtonsoft Json](https://www.newtonsoft.com/json) to get these to work.
* **Automatic111 StableDiffusion**: Follow the [install instructions on this repo](https://github.com/AUTOMATIC1111/stable-diffusion-webui), including setting up Python on Windows. Then follow [these instructions to set up the API](https://sphuff.dev/automatic-now-has-an-api).
* **Key paths:** For GPT-3/ GPT-4, put your OpenAI API key into a text file, the path of which you provide via `TextAI.key = "..."`. And if instead of a local StableDiffusion you want to use Replicate.com, add your API key with them into a file you then reference via `ImageAIReplicate.key = "..."`.
* **Cache path:** Change the cache path via `Cache.rootFolder = "..."` to a directory of your choice.

## How to use

Have a look at UniverseStart.cs to see some examples for GPT-4 and Stable Diffusion live lookups in action.

For instance, this is how to use **GPT-4/ ChatGPT** in Unity:

    async void TestTextAI()
    {
        textAI = GetComponent<TextAI>();
        string result = await textAI.GetAnswer("Who was Albert Einstein? Thanks!");
        if (!string.IsNullOrEmpty(result))
        {
            Debug.Log(result);
        }
    }

You can also await several prompts simultaneously:

    async void TestWhenAll()
    {
        Task<string> a = textAI.GetAnswer("Who was Albert Einstein?");
        Task<string> b = textAI.GetAnswer("Who is Susan Sarandon?");

        await Task.WhenAll(a, b);
        
        Debug.Log("a: " + a.Result);
        Debug.Log("b: " + b.Result);
    }

If you prefer, there's also a class available to use Coroutines instead of async-await. Text completion is further [described at OpenAI](https://beta.openai.com/docs/guides/completion). For example you can use GPT-3 to automatically generate room descriptions, things NPCs speak in chat, or [Unity C# tips](https://outer-court.com/csharp-tips/).

To grab a live Texture2D from **Stable Diffusion** for a Unity shape, use:

    void TestImageAI()
    {
        imageAI = GetComponent<ImageAI>();
        StartCoroutine(
            imageAI.GetImage("person on mountain", (Texture2D texture) =>
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
            },
            useCache: false, width: 512, height: 512
        ));
    }

You can find Stable Diffusion prompt inspiration at [Lexica.art](https://lexica.art). 

To grab an image from **Dall-E 2**, use the same as above but change to

        imageAI = GetComponent<ImageAIDallE>();

When prompting Dall-E or other APIs for something "... on black background", the following can be used to try remove that background by making it transparent (on objects where the shader is set to fade):

    Color fillColor = new Color(0f, 0f, 0.2f, 0f);
    ImageFloodFill.FillFromSides(texture, fillColor,
        threshold: 0.075f, contour: 5f, bottomAlignImage: true);
