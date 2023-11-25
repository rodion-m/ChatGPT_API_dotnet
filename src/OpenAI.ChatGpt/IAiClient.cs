using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

/// <summary>
/// AI clients interface.
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Get a chat completion response as a string
    /// </summary>
    /// <param name="dialog">The dialog history</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature">
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </param>
    /// <param name="user">
    ///  A unique identifier representing your end-user, which can help OpenAI to monitor
    ///  and detect abuse.
    /// </param>
    /// <param name="jsonMode">
    /// If true, the response will be returned as a JSON object.
    /// When using JSON mode, always instruct the model to produce JSON via some message in the conversation,
    /// for example via your system message.
    /// See: https://platform.openai.com/docs/guides/text-generation/json-mode
    /// </param>
    /// <param name="seed">
    /// If specified, our system will make a best effort to sample deterministically, such that repeated requests with the same `seed` and parameters should return the same result.
    /// Determinism is not guaranteed, and you should refer to the `system_fingerprint` response parameter to monitor changes in the backend.
    /// See: https://platform.openai.com/docs/guides/text-generation/reproducible-outputs
    /// This feature is in Beta.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="rawResponseGetter">A delegate to get the raw response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion response as a string</returns>
    Task<string> GetChatCompletions(
        UserOrSystemMessage dialog,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a chat completion response as a string
    /// </summary>
    /// <param name="messages">The dialog history</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature">
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </param>
    /// <param name="user">
    ///  A unique identifier representing your end-user, which can help OpenAI to monitor
    ///  and detect abuse.
    /// </param>
    /// <param name="jsonMode">
    /// If true, the response will be returned as a JSON object.
    /// When using JSON mode, always instruct the model to produce JSON via some message in the conversation,
    /// for example via your system message.
    /// See: https://platform.openai.com/docs/guides/text-generation/json-mode
    /// </param>
    /// <param name="seed">
    /// If specified, our system will make a best effort to sample deterministically, such that repeated requests with the same `seed` and parameters should return the same result.
    /// Determinism is not guaranteed, and you should refer to the `system_fingerprint` response parameter to monitor changes in the backend.
    /// See: https://platform.openai.com/docs/guides/text-generation/reproducible-outputs
    /// This feature is in Beta.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="rawResponseGetter">A delegate to get the raw response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion response as a string</returns>
    Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a raw chat completion response
    /// </summary>
    /// <param name="messages">The dialog history</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature">
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </param>
    /// <param name="user">
    ///  A unique identifier representing your end-user, which can help OpenAI to monitor
    ///  and detect abuse.
    /// </param>
    /// <param name="jsonMode">
    /// If true, the response will be returned as a JSON object.
    /// When using JSON mode, always instruct the model to produce JSON via some message in the conversation,
    /// for example via your system message.
    /// See: https://platform.openai.com/docs/guides/text-generation/json-mode
    /// </param>
    /// <param name="seed">
    /// If specified, our system will make a best effort to sample deterministically, such that repeated requests with the same `seed` and parameters should return the same result.
    /// Determinism is not guaranteed, and you should refer to the `system_fingerprint` response parameter to monitor changes in the backend.
    /// See: https://platform.openai.com/docs/guides/text-generation/reproducible-outputs
    /// This feature is in Beta.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The raw chat completion response</returns>
    Task<ChatCompletionResponse> GetChatCompletionsRaw(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start streaming chat completions like ChatGPT
    /// </summary>
    /// <param name="messages">The history of messaging</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="temperature">
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </param>
    /// <param name="user">
    ///  A unique identifier representing your end-user, which can help OpenAI to monitor
    ///  and detect abuse.
    /// </param>
    /// <param name="jsonMode">
    /// If true, the response will be returned as a JSON object.
    /// When using JSON mode, always instruct the model to produce JSON via some message in the conversation,
    /// for example via your system message.
    /// See: https://platform.openai.com/docs/guides/text-generation/json-mode
    /// </param>
    /// <param name="seed">
    /// If specified, our system will make a best effort to sample deterministically, such that repeated requests with the same `seed` and parameters should return the same result.
    /// Determinism is not guaranteed, and you should refer to the `system_fingerprint` response parameter to monitor changes in the backend.
    /// See: https://platform.openai.com/docs/guides/text-generation/reproducible-outputs
    /// This feature is in Beta.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Chunks of LLM's response, one by one.</returns>
    IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
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
    /// <param name="jsonMode">
    /// If true, the response will be returned as a JSON object.
    /// When using JSON mode, always instruct the model to produce JSON via some message in the conversation,
    /// for example via your system message.
    /// See: https://platform.openai.com/docs/guides/text-generation/json-mode
    /// </param>
    /// <param name="seed">
    /// If specified, our system will make a best effort to sample deterministically, such that repeated requests with the same `seed` and parameters should return the same result.
    /// Determinism is not guaranteed, and you should refer to the `system_fingerprint` response parameter to monitor changes in the backend.
    /// This feature is in Beta.
    /// See: https://platform.openai.com/docs/guides/text-generation/reproducible-outputs
    /// </param>
    /// <param name="requestModifier">Request modifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chunks of LLM's response, one by one</returns>
    IAsyncEnumerable<string> StreamChatCompletions(
        UserOrSystemMessage messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start streaming raw chat completion responses
    /// </summary>
    /// <param name="request">The chat completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream of raw chat completion responses</returns>
    IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start streaming raw chat completion responses
    /// </summary>
    /// <param name="request">The chat completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A stream of raw chat completion responses</returns>
    IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request, CancellationToken cancellationToken = default);
}