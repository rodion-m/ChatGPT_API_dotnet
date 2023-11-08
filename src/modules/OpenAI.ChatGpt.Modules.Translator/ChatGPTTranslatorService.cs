using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using OpenAI.ChatGpt.Modules.StructuredResponse;

namespace OpenAI.ChatGpt.Modules.Translator;

[Fody.ConfigureAwait(false)]
// ReSharper disable once InconsistentNaming
public class ChatGPTTranslatorService : IDisposable, IChatGPTTranslatorService
{
    private readonly IOpenAiClient _client;
    private readonly string? _defaultSourceLanguage;
    private readonly string? _defaultTargetLanguage;
    private readonly string? _extraPrompt;
    private readonly bool _isHttpClientInjected;

    public ChatGPTTranslatorService(
        IOpenAiClient client,
        string? defaultSourceLanguage = null,
        string? defaultTargetLanguage = null,
        string? extraPrompt = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _isHttpClientInjected = true;
        _defaultSourceLanguage = defaultSourceLanguage;
        _defaultTargetLanguage = defaultTargetLanguage;
        _extraPrompt = extraPrompt;
    }

    public ChatGPTTranslatorService(
        string apiKey,
        string? host = null,
        string? defaultSourceLanguage = null,
        string? defaultTargetLanguage = null,
        string? extraPrompt = null)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        _client = new OpenAiClient(apiKey, host);
        _defaultSourceLanguage = defaultSourceLanguage;
        _defaultTargetLanguage = defaultTargetLanguage;
        _extraPrompt = extraPrompt;
    }

    public void Dispose()
    {
        if (!_isHttpClientInjected && _client is IDisposable disposableClient)
        {
            disposableClient.Dispose();
        }
    }

    public async Task<string> TranslateText(
        string text,
        string? sourceLanguage = null,
        string? targetLanguage = null,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        var sourceLanguageOrDefault = sourceLanguage ?? _defaultSourceLanguage;
        var targetLanguageOrDefault = targetLanguage ?? _defaultTargetLanguage;
        if (sourceLanguageOrDefault is null)
        {
            throw new ArgumentNullException(nameof(sourceLanguage), "Source language is not specified");
        }

        if (targetLanguageOrDefault is null)
        {
            throw new ArgumentNullException(nameof(targetLanguage), "Target language is not specified");
        }

        var prompt = CreateTextTranslationPrompt(sourceLanguageOrDefault, targetLanguageOrDefault);
        var messages = Dialog.StartAsSystem(prompt).ThenUser(text).GetMessages().ToArray();
        (model, maxTokens) = ChatCompletionMessage.FindOptimalModelAndMaxToken(messages, model, maxTokens);
        var response = await _client.GetChatCompletions(
            messages,
            maxTokens.Value,
            model,
            temperature,
            user,
            false,
            null,
            requestModifier,
            rawResponseGetter,
            cancellationToken
        );
        return response;
    }

    internal virtual string CreateTextTranslationPrompt(string sourceLanguage, string targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        return $"I want you to act as a translator from {sourceLanguage} to {targetLanguage}. " +
               "The user provides with a sentence and you translate it. " +
               "In the response write ONLY translated text." +
               (_extraPrompt is not null ? "\n" + _extraPrompt : "");
    }

    public virtual async Task<TObject> TranslateObject<TObject>(
        TObject objectToTranslate,
        bool isBatch = false,
        string? sourceLanguage = null,
        string? targetLanguage = null,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        CancellationToken cancellationToken = default) where TObject : class
    {
        ArgumentNullException.ThrowIfNull(objectToTranslate);
        var sourceLanguageOrDefault = sourceLanguage ?? _defaultSourceLanguage;
        var targetLanguageOrDefault = targetLanguage ?? _defaultTargetLanguage;
        if (sourceLanguageOrDefault is null)
        {
            throw new ArgumentNullException(nameof(sourceLanguage), "Source language is not specified");
        }

        if (targetLanguageOrDefault is null)
        {
            throw new ArgumentNullException(nameof(targetLanguage), "Target language is not specified");
        }

        var prompt =
            isBatch
                ? CreateObjectTranslationPrompt(sourceLanguageOrDefault, targetLanguageOrDefault)
                : CreateBatchTranslationPrompt(sourceLanguageOrDefault, targetLanguageOrDefault);
        jsonSerializerOptions ??= new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.Never };
        var objectJson = JsonSerializer.Serialize(objectToTranslate, jsonSerializerOptions);
        var dialog = Dialog.StartAsSystem(prompt).ThenUser(objectJson);
        var messages = dialog.GetMessages().ToArray();
        (model, maxTokens) = ChatCompletionMessage.FindOptimalModelAndMaxToken(messages, model, maxTokens);
        var response = await _client.GetStructuredResponse<TObject>(
            dialog,
            maxTokens.Value,
            model,
            temperature,
            user,
            requestModifier,
            rawResponseGetter,
            jsonDeserializerOptions,
            cancellationToken: cancellationToken
        );
        return response;
    }

    internal string CreateBatchTranslationPrompt(string sourceLanguage, string targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        return $"I want you to act as a translator from {sourceLanguage} to {targetLanguage}. " +
               "The user provides you a batch of texts with an object in JSON. " +
               (_extraPrompt is not null ? "\n" + _extraPrompt : "");
    }

    internal string CreateObjectTranslationPrompt(string sourceLanguage, string targetLanguage)
    {
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        return $"I want you to act as a translator from {sourceLanguage} to {targetLanguage}. " +
               "The user provides you with an object in JSON. You translate only the text fields that need to be translated. " +
               (_extraPrompt is not null ? "\n" + _extraPrompt : "");
    }
}