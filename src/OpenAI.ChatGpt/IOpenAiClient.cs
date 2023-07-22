using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

public interface IOpenAiClient
{
    Task<string> GetChatCompletions(
        UserOrSystemMessage dialog,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default);

    Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default);

    Task<ChatCompletionResponse> GetChatCompletionsRaw(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start streaming chat completions like ChatGPT
    /// </summary>
    /// <param name="messages">The history of messaging</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature">
    ///     What sampling temperature to use, between 0 and 2.
    ///     Higher values like 0.8 will make the output more random,
    ///     while lower values like 0.2 will make it more focused and deterministic.
    /// </param>
    /// <param name="user">
    ///     A unique identifier representing your end-user, which can help OpenAI to monitor
    ///     and detect abuse.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chunks of ChatGPT's response, one by one.</returns>
    IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start streaming chat completions like ChatGPT
    /// </summary>
    /// <param name="messages">The history of messaging</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature"><see cref="ChatCompletionRequest.Temperature"/>></param>
    /// <param name="user"><see cref="ChatCompletionRequest.User"/></param>
    /// <param name="requestModifier">Request modifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chunks of ChatGPT's response, one by one</returns>
    IAsyncEnumerable<string> StreamChatCompletions(
        UserOrSystemMessage messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,CancellationToken cancellationToken = default);

    IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request, CancellationToken cancellationToken = default);
}