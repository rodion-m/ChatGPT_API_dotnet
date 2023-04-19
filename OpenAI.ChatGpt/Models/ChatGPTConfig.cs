using System.ComponentModel.DataAnnotations;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt.Models;

// ReSharper disable once InconsistentNaming
public class ChatGPTConfig
{
    public static ChatGPTConfig Default => new()
    {
        PassUserIdToOpenAiRequests = true
    };

    private int? _maxTokens;
    private string? _model;
    private float? _temperature;

    public string? InitialSystemMessage { get; set; }
    public string? InitialUserMessage { get; set; }

    public int? MaxTokens
    {
        get => _maxTokens;
        set
        {
            if (value is { } maxTokens)
            {
                if (_model is { } model)
                {
                    ChatCompletionModels.EnsureMaxTokensIsSupported(model, maxTokens);
                }
                else
                {
                    ChatCompletionModels.EnsureMaxTokensIsSupportedByAnyModel(maxTokens);
                }
            }

            _maxTokens = value;
        }
    }

    /// <summary>
    /// ID of the model to use. One of: <see cref="ChatCompletionModels"/>
    /// </summary>
    public string? Model
    {
        get => _model;
        set => _model = value != null ? ChatCompletionModels.FromString(value) : null;
    }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// Predefined values: <see cref="ChatCompletionTemperatures"/>
    /// </summary>
    [Range(ChatCompletionTemperatures.Minimum, ChatCompletionTemperatures.Maximum)]
    public float? Temperature
    {
        get => _temperature;
        set => _temperature = value is { } temp ? ChatCompletionTemperatures.Custom(temp) : null;
    }

    public bool? PassUserIdToOpenAiRequests { get; set; }

    public UserOrSystemMessage? GetInitialDialogOrNull()
    {
        return (InitialSystemMessage, InitialUserMessage) switch
        {
            (not null, not null) => Dialog
                .StartAsSystem(InitialSystemMessage)
                .ThenUser(InitialUserMessage),
            (not null, null) => Dialog.StartAsSystem(InitialSystemMessage),
            (null, not null) => Dialog.StartAsUser(InitialUserMessage),
            _ => null
        };
    }

    internal void ModifyRequest(ChatCompletionRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (_maxTokens is not null) request.MaxTokens = _maxTokens.Value;
        if (Model is not null) request.Model = Model;
        if (Temperature is not null) request.Temperature = Temperature.Value;
    }

    /// <summary>
    /// Merges two <see cref="ChatGPTConfig"/>s with respect to <paramref name="config"/>.
    /// </summary>
    public static ChatGPTConfig? Combine(
        ChatGPTConfig? baseConfig,
        ChatGPTConfig? config)
    {
        if (baseConfig is null && config is null) return null;
        if (baseConfig is null) return config;
        if (config is null) return baseConfig;

        var result = new ChatGPTConfig()
        {
            _model = config._model ?? baseConfig._model,
            _maxTokens = config._maxTokens ?? baseConfig._maxTokens,
            _temperature = config._temperature ?? baseConfig._temperature,
            PassUserIdToOpenAiRequests = config.PassUserIdToOpenAiRequests ??
                                         baseConfig.PassUserIdToOpenAiRequests,
            InitialSystemMessage = config.InitialSystemMessage ?? baseConfig.InitialSystemMessage,
            InitialUserMessage = config.InitialUserMessage ?? baseConfig.InitialUserMessage
        };
        return result;
    }

    public static ChatGPTConfig CombineOrDefault(
        ChatGPTConfig? baseConfig, ChatGPTConfig? config)
    {
        return Combine(baseConfig, config) ?? Default;
    }
}