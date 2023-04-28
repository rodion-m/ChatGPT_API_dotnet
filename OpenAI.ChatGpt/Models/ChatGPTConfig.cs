using System.ComponentModel.DataAnnotations;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt.Models;

// ReSharper disable once InconsistentNaming
public class ChatGPTConfig
{
    /// <summary>Default configuration.</summary>
    public static ChatGPTConfig Default => new()
    {
        PassUserIdToOpenAiRequests = true
    };

    private int? _maxTokens;
    private string? _model;
    private float? _temperature;

    /// <summary>
    /// This is a system message, that will be sent to OpenAI API as a first message.
    /// Initial dialog to start with, that allows to fine-tune the chatbot (message from system).
    /// </summary>
    /// <example>
    /// English teacher prompt:
    /// ```
    /// I want you to act as an English translator, spelling corrector and improver.
    /// I will speak to you in any language and you will detect the language,
    /// translate it and answer in the corrected and improved version of my text, in English.
    /// I want you to replace my simplified A0-level words and sentences with
    /// more beautiful and elegant, upper level English words and sentences.
    /// Keep the meaning same, but make them more literary.
    /// I want you to only reply the correction, the improvements and nothing else,
    /// do not write explanations.
    /// My first sentence is “istanbulu cok seviyom burada olmak cok guzel”
    /// ```
    /// See more prompts here: https://prompts.chat/
    /// </example>
    /// <remarks>
    /// If <see cref="InitialSystemMessage"/> and <see cref="InitialUserMessage"/> are provided,
    /// then both messages will be send to OpenAI API.
    /// More info about initial message: https://github.com/openai/openai-python/blob/main/chatml.md
    /// </remarks>
    public string? InitialSystemMessage { get; set; }
    
    /// <summary>
    /// This is a user message, that will be sent to OpenAI API as a first message.
    /// Initial dialog to start with, that allows to fine-tune the chatbot (message from the user).
    /// <see cref="InitialSystemMessage"/>
    /// </summary>
    /// <remarks>
    /// If <see cref="InitialSystemMessage"/> and <see cref="InitialUserMessage"/> are provided,
    /// then both messages will be send to OpenAI API.
    /// More info about initial message: https://github.com/openai/openai-python/blob/main/chatml.md
    /// </remarks>
    public string? InitialUserMessage { get; set; }

    /// <summary>
    /// The maximum number of tokens allowed for the generated answer.
    /// Defaults to <see cref="ChatCompletionRequest.MaxTokensDefault"/>.
    /// This value is validated and limited with <see cref="ChatCompletionModels.GetMaxTokensLimitForModel"/> method.
    /// It's possible to calculate approximately tokens count using <see cref="ChatCompletionMessage.CalculateApproxTotalTokenCount()"/> method.
    /// Maps to: <see cref="ChatCompletionRequest.MaxTokens"/>
    /// </summary>
    /// <remarks>
    /// The number of tokens can be retrieved from the API response: <see cref="ChatCompletionResponse.Usage"/>
    /// As a rule of thumb for English, 1 token is around 4 characters (so 100 tokens ≈ 75 words).
    /// See: https://platform.openai.com/tokenizer
    /// Encoding algorithm can be found here: https://github.com/latitudegames/GPT-3-Encoder
    /// </remarks>
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
    /// Maps to: <see cref="ChatCompletionRequest.Model"/>
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
    /// Maps to: <see cref="ChatCompletionRequest.Temperature"/>
    /// </summary>
    [Range(ChatCompletionTemperatures.Minimum, ChatCompletionTemperatures.Maximum)]
    public float? Temperature
    {
        get => _temperature;
        set => _temperature = value is { } temp ? ChatCompletionTemperatures.Custom(temp) : null;
    }

    /// <summary>
    /// Whether to include the user ID into OpenAI requests.
    /// See also: <see cref="ChatCompletionRequest.User"/>
    /// </summary>
    /// <remarks>
    /// More info about users: https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids
    /// </remarks>
    public bool? PassUserIdToOpenAiRequests { get; set; }

    /// <summary>
    /// Returns initial dialog to start with, that allows to fine-tune the chatbot.
    /// </summary>
    public virtual UserOrSystemMessage? GetInitialDialogOrNull()
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

    /// <summary>
    /// Merges two <see cref="ChatGPTConfig"/>s with respect to <paramref name="config"/>.
    /// If both <paramref name="baseConfig"/> and <paramref name="config"/> are null, then returns <see cref="Default"/>.
    /// </summary>
    public static ChatGPTConfig CombineOrDefault(ChatGPTConfig? baseConfig, ChatGPTConfig? config)
    {
        return Combine(baseConfig, config) ?? Default;
    }
}