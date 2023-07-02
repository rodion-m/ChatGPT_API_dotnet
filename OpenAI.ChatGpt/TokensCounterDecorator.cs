using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

public class TokensCounterDecorator : IOpenAiClient, IDisposable
{
    private readonly IOpenAiClient _decorated;

    public long TotalTokensCount { get; private set; }


    public TokensCounterDecorator(IOpenAiClient decorated)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public async Task<string> GetChatCompletions(
        UserOrSystemMessage dialog,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        return await _decorated.GetChatCompletions(dialog, maxTokens, model, temperature, user,
            requestModifier, WrapRawResponseGetter(rawResponseGetter), cancellationToken);
    }

    public async Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        return await _decorated.GetChatCompletions(messages, maxTokens, model, temperature, user,
            requestModifier, WrapRawResponseGetter(rawResponseGetter), cancellationToken);
    }

    private Action<ChatCompletionResponse> WrapRawResponseGetter(
        Action<ChatCompletionResponse>? rawResponseGetter)
    {
        return response =>
        {
            TotalTokensCount += response.Usage.TotalTokens;
            rawResponseGetter?.Invoke(response);
        };
    }

    public void Dispose()
    {
        if(_decorated is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async Task<ChatCompletionResponse> GetChatCompletionsRaw(
        IEnumerable<ChatCompletionMessage> messages, int maxTokens = 64,
        string model = "gpt-3.5-turbo",
        float temperature = 0.5f, string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        return await _decorated.GetChatCompletionsRaw(messages, maxTokens, model, temperature, user,
            requestModifier, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages, int maxTokens = 64,
        string model = "gpt-3.5-turbo", float temperature = 0.5f, string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        return _decorated.StreamChatCompletions(messages, maxTokens, model, temperature, user,
            requestModifier, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamChatCompletions(UserOrSystemMessage messages,
        int maxTokens = 64,
        string model = "gpt-3.5-turbo", float temperature = 0.5f, string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _decorated.StreamChatCompletions(messages, maxTokens, model, temperature, user,
            requestModifier, cancellationToken);
    }

    public IAsyncEnumerable<string> StreamChatCompletions(ChatCompletionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _decorated.StreamChatCompletions(request, cancellationToken);
    }

    public IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _decorated.StreamChatCompletionsRaw(request, cancellationToken);
    }
}