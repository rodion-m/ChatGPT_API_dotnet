using System.Text.Json;
using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Modules.Translator;

[Fody.ConfigureAwait(false)]
public static class OpenAiClientExtensions
{
    public static Task<string> TranslateText(
        this IOpenAiClient client,
        string text,
        string sourceLanguage,
        string targetLanguage,
        string? extraPrompt = null,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        
        var translator = new ChatGPTTranslatorService(client, extraPrompt: extraPrompt);
        return translator.TranslateText(
            text,
            sourceLanguage,
            targetLanguage,
            maxTokens,
            model,
            temperature,
            user,
            requestModifier,
            rawResponseGetter,
            cancellationToken);
    }
    
    public static Task<TObject> TranslateObject<TObject>(
        this IOpenAiClient client,
        TObject objectToTranslate,
        string sourceLanguage,
        string targetLanguage,
        string? extraPrompt = null,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        JsonSerializerOptions? jsonSerializerOptions = null,
        JsonSerializerOptions? jsonDeserializerOptions = null,
        CancellationToken cancellationToken = default) where TObject: class
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(objectToTranslate);
        ArgumentNullException.ThrowIfNull(sourceLanguage);
        ArgumentNullException.ThrowIfNull(targetLanguage);
        
        var translator = new ChatGPTTranslatorService(client, extraPrompt: extraPrompt);
        return translator.TranslateObject(
            objectToTranslate,
            sourceLanguage,
            targetLanguage,
            maxTokens,
            model,
            temperature,
            user,
            requestModifier,
            rawResponseGetter,
            jsonSerializerOptions,
            jsonDeserializerOptions,
            cancellationToken
        );
    }
}