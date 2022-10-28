using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ImageAIParamsWrapperReplicate
{
    // Parameters wrapper when prompting Replicate Stable-Diffusion model
    // https://replicate.com/stability-ai/stable-diffusion

    const string newestVersionAtTimeOfWriting = "a9758cbfbd5f3c2094457d996681af52552901775aa2d6dd0b17fd15df959bef";

    public string version = newestVersionAtTimeOfWriting;
    public ImageAIParamsReplicate input = null;

}
