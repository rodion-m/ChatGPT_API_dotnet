using System.Text.Json;
using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Modules.Translator;

public interface IChatGPTTranslatorService
{
    Task<string> TranslateText(
        string text,
        string? sourceLanguage = null,
        string? targetLanguage = null,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default);

    Task<TObject> TranslateObject<TObject>(
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
        CancellationToken cancellationToken = default) where TObject : class;
}