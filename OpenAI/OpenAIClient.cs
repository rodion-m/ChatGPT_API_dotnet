using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Exceptions;
using OpenAI.Models.ChatCompletion;
using OpenAI.Models.Images;

namespace OpenAI;

/// <summary> Thread-safe OpenAI client. </summary>
public class OpenAiClient : IDisposable
{
    private const string DefaultHost = "https://api.openai.com/v1/";
    private const string ImagesEndpoint = "images/generations";
    private const string ChatCompletionsEndpoint = "chat/completions";

    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _nullIgnoreSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public OpenAiClient(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(apiKey));
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(DefaultHost)
        };
        var header = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Authorization = header;
    }

    public void Dispose() => _httpClient.Dispose();

    public async Task<string> GetChatCompletions(
        UserMessage dialog,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        CancellationToken cancellationToken = default)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        if (model == null) throw new ArgumentNullException(nameof(model));
        var res = await GetChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = dialog.GetMessages()
        }, cancellationToken);
        return res.Choices[0].Message!.Content;
    }

    public async Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        var res = await GetChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages
        }, cancellationToken);
        return res.Choices[0].Message!.Content;
    }

    internal async Task<ChatCompletionResponse> GetChatCompletions(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        var response = await _httpClient.PostAsJsonAsync(
            ChatCompletionsEndpoint,
            request,
            cancellationToken: cancellationToken,
            options: _nullIgnoreSerializerOptions
        ).ConfigureAwait(false);
        var responseContent = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            ThrowNotExpectedResponseException(response.StatusCode, responseContent);
        }

        var jsonResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent)!;
        return jsonResponse;
    }

    private static void ThrowNotExpectedResponseException(
        HttpStatusCode statusCode,
        string responseContent) 
        => throw new NotExpectedResponseException(statusCode, responseContent);

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
    /// A unique identifier representing your end-user, which can help OpenAI to monitor
    /// and detect abuse.
    /// </param>
    /// <param name="requestModifier">A modifier of the raw request. Allows to specify any custom properties.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chunks of ChatGPT's response, one by one.</returns>
    public IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        var request = new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages,
            Stream = true,
            User = user,
            Temperature = temperature
        };
        requestModifier?.Invoke(request);
        return StreamChatCompletions(request, cancellationToken);
    }

    /// <summary>
    /// Start streaming chat completions like ChatGPT
    /// </summary>
    /// <param name="messages">The history of messaging</param>
    /// <param name="maxTokens">The length of the response</param>
    /// <param name="model">One of <see cref="ChatCompletionModels"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chunks of ChatGPT's response, one by one</returns>
    public IAsyncEnumerable<string> StreamChatCompletions(
        UserMessage messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        return StreamChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages.GetMessages(),
            Stream = true
        }, cancellationToken);
    }

    public async IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Stream = true;
        await foreach (var response in StartStreaming().WithCancellation(cancellationToken))
        {
            var content = response!.Choices[0].Delta?.Content;
            if (content is not null)
                yield return content;
        }

        IAsyncEnumerable<ChatCompletionResponse> StartStreaming()
        {
            return _httpClient.StreamUsingServerSentEvents<
                ChatCompletionRequest, ChatCompletionResponse>
            (
                ChatCompletionsEndpoint,
                request,
                _nullIgnoreSerializerOptions,
                cancellationToken
            );
        }
    }

    public async Task<byte[]> GenerateImageBytes(
        string prompt,
        string? user = null,
        OpenAiImageSize size = OpenAiImageSize._1024,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(prompt));
        var request = new ImageGenerationRequest(prompt, SizeToString(size), 1, "b64_json", user);
        var response = await _httpClient.PostAsJsonAsync(
            ImagesEndpoint,
            request,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to generate image: {responseContent}");
        }

        var jsonResponse =
            JsonSerializer.Deserialize<ImagesGenerationB64Response>(responseContent)!;
        return Convert.FromBase64String(jsonResponse.Data[0].B64Json);
    }

    public async Task<Uri[]> GenerateImagesUris(
        string prompt,
        string? user = null,
        OpenAiImageSize size = OpenAiImageSize._1024,
        int count = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(prompt));
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        var request = new ImageGenerationRequest(prompt, SizeToString(size), count, "url", user);
        var response = await _httpClient.PostAsJsonAsync(
            ImagesEndpoint,
            request,
            options: _nullIgnoreSerializerOptions,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to generate image: {responseContent}");
        }

        var jsonResponse =
            JsonSerializer.Deserialize<ImagesGenerationUriResponse>(responseContent)!;
        return jsonResponse.Data.Select(it => it.Url).ToArray();
    }

    private static string SizeToString(OpenAiImageSize size)
    {
        return size switch
        {
            OpenAiImageSize._256 => "256x256",
            OpenAiImageSize._512 => "512x512",
            OpenAiImageSize._1024 => "1024x1024",
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
        };
    }
}