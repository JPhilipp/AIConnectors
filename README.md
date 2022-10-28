# AIConnectors
 **Here you'll find Unity C# connections to StableDiffusion, GPT-3, and possibly other AI tools in the future.**

🡆 To see these in action, check out the [AI TV channel on YouTube](https://www.youtube.com/playlist?list=PL9LD6IUjh-8iQZ-cvfTYwnuzEDECCqrRr).

---


https://user-images.githubusercontent.com/1754503/198673075-a051cbd3-9a68-4edb-9eba-85f30abac2b0.mp4


**Stable Diffusion** is a machine learning AI that turns text (or images) into images. For instance, "knight in shiny armor, fantasy art" becomes a glorious 1024x768 painting in seconds. You'll find a class to connect Unity to a local API. Alternatively, you can also connect to the paid [Replicate.com](https://replicate.com) with your account there.

---

**GPT-3** is a deep learning artificial intelligence which continues a given text prompt. For instance, as you can try for yourself in the [OpenAI playground](https://beta.openai.com/playground), the text "Albert Einstein was" gets completed with "born in Ulm, in the Kingdom of Württemberg in the German Empire, on 14 March 1879. His parents were ..." and more.

---

## How to install

* **Json:** Please grab [Newtonsoft Json](https://www.newtonsoft.com/json) to get these to work.
* **Automatic111 StableDiffusion**: Follow the [install instructions on this repo](https://github.com/AUTOMATIC1111/stable-diffusion-webui). Then follow [these instructions to set up the API](https://sphuff.dev/automatic-now-has-an-api).
* **Key paths:** These classes read local key files, the path of which you can change via `ImageAI.key = "..."` and `TextAI.key = "..."` as shown in UniverseStart.cs.
* **Cache path:** You also want to change the cache path near the top of GlobalUse/Cache.cs.
