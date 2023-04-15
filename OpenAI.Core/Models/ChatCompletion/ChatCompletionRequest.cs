using System.Text.Json.Serialization;
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618
namespace OpenAI.Models.ChatCompletion;

/// <summary>
/// Represents the API request body for generating chat completions.
/// </summary>
/// <remarks>
/// See: https://platform.openai.com/docs/api-reference/chat/create
/// </remarks>
public class ChatCompletionRequest
{
    public const int MaxTokensDefault = 64;
    
    private int _maxTokens = MaxTokensDefault;
    private string _model = ChatCompletionModels.Default;
    private float _temperature = ChatCompletionTemperatures.Default;
    private IEnumerable<ChatCompletionMessage> _messages;

    public ChatCompletionRequest(IEnumerable<ChatCompletionMessage> messages)
    {
        _messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    /// <summary>
    /// ID of the model to use. One of: <see cref="ChatCompletionModels"/>
    /// </summary>
    [JsonPropertyName("model")]
    public string Model
    {
        get => _model;
        set => _model = ChatCompletionModels.FromString(value);
    }

    /// <summary>
    /// The messages to generate chat completions for, in the ChatML format.
    /// About ChatML: https://github.com/openai/openai-python/blob/main/chatml.md
    /// </summary>
    [JsonPropertyName("messages")]
    public IEnumerable<ChatCompletionMessage> Messages
    {
        get => _messages;
        set => _messages = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float Temperature
    {
        get => _temperature;
        set => _temperature = ChatCompletionTemperatures.VaidateTemperature(value);
    }

    /// <summary>
    /// An alternative to sampling with temperature, called nucleus sampling,
    /// where the model considers the results of the tokens with top_p probability mass.
    /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// How many chat completion choices to generate for each input message.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; } = 1;

    /// <summary>
    /// If set, partial message deltas will be sent, like in ChatGPT. Tokens will be sent as data-only server-sent events as they become available, with the stream terminated by a data: [DONE] message.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    /// <summary>
    /// Up to 4 sequences where the API will stop generating further tokens.
    /// </summary>
    [JsonPropertyName("stop")]
    public List<string>? Stop { get; set; }

    /// <summary>
    /// The maximum number of tokens allowed for the generated answer.
    /// Defaults to <see cref="MaxTokensDefault"/>.
    /// This value is validated and limited with <see cref="ChatCompletionModels.GetMaxTokensLimitForModel"/> meghod.
    /// It's possible to calculate approximately tokens count using <see cref="ChatCompletionMessage.CalculateApproxTotalTokenCount()"/> method.
    /// </summary>
    /// <remarks>
    /// The number of tokens can be retrieved from the API response: <see cref="ChatCompletionResponse.Usage"/>
    /// As a rule of thumb for English, 1 token is around 4 characters (so 100 tokens ≈ 75 words).
    /// See: https://platform.openai.com/tokenizer
    /// Encoding algorithm can be found here: https://github.com/latitudegames/GPT-3-Encoder
    /// </remarks>
    [JsonPropertyName("max_tokens")]
    public int MaxTokens
    {
        get => _maxTokens;
        set
        {
            ChatCompletionModels.EnsureMaxTokensIsSupported(Model, value);
            _maxTokens = value;
        }
    }

    /// <summary>
    /// Number between -2.0 and 2.0.
    /// Positive values penalize new tokens based on whether they appear in the text so far,
    /// increasing the model's likelihood to talk about new topics.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0.
    /// Positive values penalize new tokens based on their existing frequency in the text so far,
    /// decreasing the model's likelihood to repeat the same line verbatim.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    /// <summary>
    /// Modify the likelihood of specified tokens appearing in the completion.
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<string, int>? LogitBias { get; set; }

    /// <summary>
    /// A unique identifier representing your end-user,
    /// which can help OpenAI to monitor
    /// and detect abuse.
    /// </summary>
    /// <remarks>
    /// See: https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids
    /// </remarks>
    [JsonPropertyName("user")]
    public string? User { get; set; }
}