using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Modules.Translator;

public class ChatGPTTranslatorService : IDisposable
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
    
    public async Task<string> Translate(
        string text,
        string? sourceLanguage = null,
        string? targetLanguage = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
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
        var prompt = GetPrompt(sourceLanguageOrDefault, targetLanguageOrDefault);
        var response = await _client.GetChatCompletions(
            Dialog.StartAsSystem(prompt).ThenUser(text),
            user: null,
            requestModifier: requestModifier,
            cancellationToken: cancellationToken
        );
        return response;
    }

    private string GetPrompt(string sourceLanguage, string targetLanguage)
    {
        return $"I want you to act as a translator from {sourceLanguage} to {targetLanguage}. " +
               "I will provide you with an English sentence and you will translate it into Russian. " +
               "In the response write ONLY translated text."
            + (_extraPrompt is not null ? "\n" + _extraPrompt : "");
    }
}