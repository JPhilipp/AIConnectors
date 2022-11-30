using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TextAIParams
{
    // Parameters when prompting OpenAI's GPT-3.
    // https://beta.openai.com/docs/api-reference/introduction


    // The string that comes before the completion.
    public string prompt = null;

    // The string that follows after the completion; in the Playground, this feature is the
    // [insert] placeholder. Currently requires model "text-davinci-002".
    public string suffix = null;

    // At the time of writing, "davinci-003" is the most capable (and costly) model.
    // https://beta.openai.com/docs/models/overview
    public const string defaultModel = "text-davinci-003";
    public string model = defaultModel;

    // "Controls randomness: Lowering results in less random completions. As the temperature
    // approaches zero, the model will become deterministic and repetitive."
    // "Defaults to 1" (Playground default: 0.7)
    public float temperature = 0.7f;

    // "The maximum number of tokens to generate. Requests can use up to 2,048 or
    // [for text-davinci-002] 4,000 tokens shared between prompt and completion. The exact limit
    // varies by model. (One token is roughly 4 characters for normal English text) ...
    // Defaults to 16" (Playground default: 256)
    [JsonProperty("max_tokens")]
    public int maxTokens = 256;

    // "Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they
    // appear in the text so far, increasing the model's likelihood to talk about new topics. ...
    // Defaults to 0"
    [JsonProperty("presence_penalty")]
    public float presencePenalty = 0f;

    // "Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing
    // frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim. ...
    // Defaults to 0"
    [JsonProperty("frequency_penalty")]
    public float frequencyPenalty = 0f;

    // "Up to 4 sequences where the API will stop generating further tokens. The returned text will not
    // contain the stop sequence."
    public string[] stop = null;

    
    // The following are not tested or used so far.

    // "Echo back the prompt in addition to the completion"
    public bool echo = false;

    // "Whether to stream back partial progress. If set, tokens will be sent as data-only server-sent
    // events as they become available, with the stream terminated by a data: [DONE] message."
    public bool stream = false;

    // "Modify the likelihood of specified tokens appearing in the completion. ...
    // As an example, you can pass {"50256": -100} to prevent the <|endoftext|> token from being
    // generated."
    [JsonProperty("logit_bias")]
    public Dictionary<string,int> logitBias = null;

    // "Include the log probabilities on the logprobs most likely tokens, as well the chosen tokens.
    // For example, if logprobs is 5, the API will return a list of the 5 most likely tokens."
    [JsonProperty("logprobs")]
    public int? logProbs = null;

    // "How many completions to generate for each prompt. Note: Because this parameter generates
    // many completions, it can quickly consume your token quota. Use carefully and ensure that
    // you have reasonable settings for max_tokens and stop."
    public int n = 0;

    // "An alternative to sampling with temperature, called nucleus sampling, where the model considers
    // the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising
    // the top 10% probability mass are considered. We generally recommend altering this or temperature
    // but not both."
    [JsonProperty("top_p")]
    public int topP = 0;

    // "Generates best_of completions server-side and returns the "best" (the one with the highest log
    // probability per token). Results cannot be streamed. When used with n, best_of controls the number
    // of candidate completions and n specifies how many to return – best_of must be greater than n.
    // Note: Because this parameter generates many completions, it can quickly consume your token quota.
    // Use carefully and ensure that you have reasonable settings for max_tokens and stop."
    [JsonProperty("best_of")]
    public int bestOf = 0;

    public void CapMaxTokensToAllowed()
    {
        int maxTokensAllowed = GetMaxTokensAllowed();
        if (maxTokens > maxTokensAllowed)
        {
            maxTokens = maxTokensAllowed;
        }
    }

    int GetMaxTokensAllowed()
    {
        return model == "text-davinci-002" ? 4000 : 2048;
    }
}
