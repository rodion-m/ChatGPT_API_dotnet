using System.Text.Json.Serialization;

#pragma warning disable CS8618
namespace OpenAI.Models.ChatCompletion;

/// <summary>
/// Represents the API request body for generating chat completions.
/// </summary>
public class ChatCompletionRequest
{
    public const float TemperatureDefault = 1f;
    public const int MaxTokensLimit = 4096;
    public const int MaxTokensDefault = 32;
    
    private int _maxTokens = MaxTokensDefault;

    /// <summary>
    /// ID of the model to use. Currently, only gpt-3.5-turbo and gpt-3.5-turbo-0301 are supported.
    /// <see cref="ChatCompletionModels"/>
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// The messages to generate chat completions for, in the chat format.
    /// </summary>
    [JsonPropertyName("messages")]
    public IEnumerable<ChatCompletionMessage> Messages { get; set; }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = TemperatureDefault;
    
    /// <summary>
    /// An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 1;

    /// <summary>
    /// How many chat completion choices to generate for each input message.
    /// </summary>
    [JsonPropertyName("n")]
    public int N { get; set; } = 1;

    /// <summary>
    /// If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available, with the stream terminated by a data: [DONE] message.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    /// <summary>
    /// Up to 4 sequences where the API will stop generating further tokens.
    /// </summary>
    [JsonPropertyName("stop")]
    public List<string> Stop { get; set; }

    /// <summary>
    /// The maximum number of tokens allowed for the generated answer. By default,
    /// the number of tokens the model can return will be 4096.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int MaxTokens
    {
        get => _maxTokens;
        set
        {
            if (value > MaxTokensLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxTokens),
                    "The maximum number of tokens allowed for the generated answer is 4096.");
            }
            _maxTokens = value;
        }
    }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; }

    /// <summary>
    /// Modify the likelihood of specified tokens appearing in the completion.
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int> LogitBias { get; set; }

    /// <summary>
    /// A unique identifier representing your end-user, which can help OpenAI to monitor
    /// and detect abuse.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}