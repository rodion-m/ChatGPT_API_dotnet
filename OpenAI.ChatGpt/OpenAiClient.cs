using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Exceptions;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using OpenAI.ChatGpt.Models.Images;

namespace OpenAI.ChatGpt;

/// <summary> Thread-safe OpenAI client. </summary>
[Fody.ConfigureAwait(false)]
public class OpenAiClient : IOpenAiClient, IDisposable
{
    private const string DefaultHost = "https://api.openai.com/v1/";
    private const string ImagesEndpoint = "images/generations";
    private const string ChatCompletionsEndpoint = "chat/completions";
    
    private static readonly Uri DefaultHostUri = new(DefaultHost);

    private readonly HttpClient _httpClient;
    private readonly bool _isHttpClientInjected;
    private bool _disposed;

    private readonly JsonSerializerOptions _nullIgnoreSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    ///  Creates a new OpenAI client with given <paramref name="apiKey"/>.
    /// </summary>
    /// <param name="apiKey">OpenAI API key. Can be issued here: https://platform.openai.com/account/api-keys</param>
    /// <param name="host">Open AI API host. Default is: <see cref="DefaultHost"/></param>
    public OpenAiClient(string apiKey, string? host = DefaultHost)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or whitespace.", nameof(apiKey));
        var uri = ValidateHost(host);
        
        _httpClient = new HttpClient()
        {
            BaseAddress = uri
        };
        var header = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Authorization = header;
    }

    /// <summary>
    /// Creates a new OpenAI client from DI with given <paramref name="httpClient"/>.
    /// </summary>
    /// <param name="httpClient">
    /// <see cref="HttpClient"/> from DI. It should have an Authorization header set with OpenAI API key.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Indicates that OpenAI API key is not set in
    /// <paramref name="httpClient"/>.<see cref="HttpClient.DefaultRequestHeaders"/>.<see cref="HttpRequestHeaders.Authorization"/> header.
    /// </exception>
    public OpenAiClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ValidateHttpClient(httpClient);
        _isHttpClientInjected = true;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!_isHttpClientInjected)
        {
            _httpClient.Dispose();
        }
        GC.SuppressFinalize(this);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().ToString());
        }
    }

    private static Uri ValidateHost(string? host)
    {
        if (host is null) return DefaultHostUri;
        if (!Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Host must be a valid absolute URI and end with a slash." +
                                        $"For example: {DefaultHost}", nameof(host));
        }
        if(!host.EndsWith("/")) uri = new Uri(host + "/");

        return uri;
    }
    
    private static void ValidateHttpClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        if (httpClient.DefaultRequestHeaders.Authorization is null)
        {
            throw new ArgumentException(
                "HttpClient must have an Authorization header set." +
                "It should include OpenAI's API key.",
                nameof(httpClient)
            );
        }

        if (httpClient.BaseAddress is null)
        {
            throw new ArgumentException(
                "HttpClient must have a BaseAddress set." +
                "It should be set to OpenAI's API endpoint.",
                nameof(httpClient)
            );
        }
        if(!httpClient.BaseAddress.AbsoluteUri.EndsWith("/"))
        {
            throw new ArgumentException(
                "HttpClient's BaseAddress must end with a slash." +
                "It should be set to OpenAI's API endpoint.",
                nameof(httpClient)
            );
        }
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
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        if (model == null) throw new ArgumentNullException(nameof(model));
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            dialog.GetMessages(),
            maxTokens,
            model,
            temperature,
            user,
            false,
            requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        rawResponseGetter?.Invoke(response);
        return response.Choices[0].Message!.Content;
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
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            false,
            requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        rawResponseGetter?.Invoke(response);
        return response.GetMessageContent();
    }
    
    public async Task<ChatCompletionResponse> GetChatCompletionsRaw(
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
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            false,
            requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        return response;
    }

    internal async Task<ChatCompletionResponse> GetChatCompletionsRaw(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ThrowIfDisposed();
        var response = await _httpClient.PostAsJsonAsync(
            ChatCompletionsEndpoint,
            request,
            cancellationToken: cancellationToken,
            options: _nullIgnoreSerializerOptions
        );
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new NotExpectedResponseException(response.StatusCode, responseContent);
        }

        var jsonResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent)!;
        return jsonResponse;
    }

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
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            true,
            requestModifier
        );
        return StreamChatCompletions(request, cancellationToken);
    }

    private static ChatCompletionRequest CreateChatCompletionRequest(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens,
        string model,
        float temperature,
        string? user,
        bool stream,
        Action<ChatCompletionRequest>? requestModifier)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var request = new ChatCompletionRequest(messages)
        {
            Model = model,
            MaxTokens = maxTokens,
            Stream = stream,
            User = user,
            Temperature = temperature
        };
        requestModifier?.Invoke(request);
        return request;
    }

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
    public IAsyncEnumerable<string> StreamChatCompletions(
        UserOrSystemMessage messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(messages.GetMessages(),
            maxTokens,
            model,
            temperature,
            user,
            true,
            requestModifier
        );
        return StreamChatCompletions(request, cancellationToken);
    }

    public async IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        ThrowIfDisposed();
        request.Stream = true;
        await foreach (var response in StreamChatCompletionsRaw(request, cancellationToken))
        {
            var content = response.Choices[0].Delta?.Content;
            if (content is not null)
                yield return content;
        }
    }

    public IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();
        request.Stream = true;
        return _httpClient.StreamUsingServerSentEvents<ChatCompletionRequest, ChatCompletionResponse>
        (
            ChatCompletionsEndpoint,
            request,
            _nullIgnoreSerializerOptions,
            cancellationToken
        );
    }

    // Will be moved to a separate package.
    internal async Task<byte[]> GenerateImageBytes(
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
        );
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new NotExpectedResponseException(response.StatusCode, responseContent);
        }

        var jsonResponse =
            JsonSerializer.Deserialize<ImagesGenerationB64Response>(responseContent)!;
        return Convert.FromBase64String(jsonResponse.Data[0].B64Json);
    }

    // Will be moved to a separate package.
    internal async Task<Uri[]> GenerateImagesUris(
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
        );
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new NotExpectedResponseException(response.StatusCode, responseContent);
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