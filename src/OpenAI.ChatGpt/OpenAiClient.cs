﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Exceptions;
using OpenAI.ChatGpt.Extensions;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

/// <summary>Thread-safe OpenAI client.</summary>
/// <remarks>https://github.com/openai/openai-openapi/blob/master/openapi.yaml</remarks>
[Fody.ConfigureAwait(false)]
public class OpenAiClient : IAiClient, IDisposable
{
    internal const string DefaultHost = "https://api.openai.com/v1/";
    private const string ChatCompletionsEndpoint = "chat/completions";
    
    private static readonly Uri DefaultHostUri = new(DefaultHost);

    protected HttpClient HttpClient;
    protected bool IsHttpClientInjected;
    private bool _disposed;

    private readonly JsonSerializerOptions _nullIgnoreSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected OpenAiClient()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

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
        
        HttpClient = new HttpClient()
        {
            BaseAddress = uri
        };
        var header = new AuthenticationHeaderValue("Bearer", apiKey);
        HttpClient.DefaultRequestHeaders.Authorization = header;
    }

    /// <summary>
    /// Creates a new OpenAI client from DI with given <paramref name="httpClient"/>.
    /// </summary>
    /// <param name="httpClient">
    /// <see cref="System.Net.Http.HttpClient"/> from DI. It should have an Authorization header set with OpenAI API key.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Indicates that OpenAI API key is not set in
    /// <paramref name="httpClient"/>.<see cref="HttpClient.DefaultRequestHeaders"/>.<see cref="HttpRequestHeaders.Authorization"/> header.
    /// </exception>
    public OpenAiClient(HttpClient httpClient) 
        : this(httpClient, true, true)
    {
    }
    
    internal OpenAiClient(
        HttpClient httpClient,         
        bool validateAuthorizationHeader, 
        bool validateBaseAddress)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        ValidateHttpClient(httpClient, validateAuthorizationHeader, validateBaseAddress);
        IsHttpClientInjected = true;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!IsHttpClientInjected)
        {
            HttpClient.Dispose();
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
            throw new ArgumentException(
                $"Host must be a valid absolute URI and end with a slash. Provided: {host}" +
                        $"\nCorrect example: {DefaultHost}", nameof(host));
        }
        if(!host.EndsWith("/")) uri = new Uri(host + "/");

        return uri;
    }
    
    private static void ValidateHttpClient(
        HttpClient httpClient, 
        bool validateAuthorizationHeader, 
        bool validateBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        if (validateAuthorizationHeader && httpClient.DefaultRequestHeaders.Authorization is null)
        {
            throw new ArgumentException(
                "HttpClient must have an Authorization header set. " +
                "It should include OpenAI's API key.",
                nameof(httpClient)
            );
        }

        if (validateBaseAddress)
        {
            if (httpClient.BaseAddress is null)
            {
                throw new ArgumentException(
                    "HttpClient must have a BaseAddress set." +
                    "It should be set to AI service API endpoint.",
                    nameof(httpClient)
                );
            }

            if (!httpClient.BaseAddress.AbsoluteUri.EndsWith("/"))
            {
                throw new ArgumentException(
                    "HttpClient's BaseAddress must end with a slash.",
                    nameof(httpClient)
                );
            }
        }
    }

    /// <inheritdoc />
    public async Task<string> GetChatCompletions(
        UserOrSystemMessage dialog,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        if (dialog == null) throw new ArgumentNullException(nameof(dialog));
        if (model == null) throw new ArgumentNullException(nameof(model));
        EnsureJsonModeIsSupported(model, jsonMode);
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            dialog.GetMessages(),
            maxTokens,
            model,
            temperature,
            user,
            jsonMode,
            seed,
            false,
            requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        rawResponseGetter?.Invoke(response);
        return response.Choices[0].Message!.Content;
    }

    /// <inheritdoc />
    public async Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        Action<ChatCompletionResponse>? rawResponseGetter = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        EnsureJsonModeIsSupported(model, jsonMode);
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            jsonMode,
            seed,
            stream: false,
            requestModifier: requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        rawResponseGetter?.Invoke(response);
        return response.GetMessageContent();
    }
    
    /// <inheritdoc />
    public async Task<ChatCompletionResponse> GetChatCompletionsRaw(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        EnsureJsonModeIsSupported(model, jsonMode);
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            jsonMode,
            seed,
            stream: false,
            requestModifier: requestModifier
        );
        var response = await GetChatCompletionsRaw(request, cancellationToken);
        return response;
    }

    internal async Task<ChatCompletionResponse> GetChatCompletionsRaw(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfDisposed();
        var response = await HttpClient.PostAsJsonAsync(
            GetChatCompletionsEndpoint(),
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

    protected virtual string GetChatCompletionsEndpoint()
    {
        return ChatCompletionsEndpoint;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = 0,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        EnsureJsonModeIsSupported(model, jsonMode);
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages,
            maxTokens,
            model,
            temperature,
            user,
            jsonMode,
            seed,
            stream: true,
            requestModifier: requestModifier
        );
        return StreamChatCompletions(request, cancellationToken);
    }

    private ChatCompletionRequest CreateChatCompletionRequest(
        IEnumerable<ChatCompletionMessage> messages,
        int? maxTokens,
        string model,
        float temperature,
        string? user,
        bool jsonMode,
        long? seed,
        bool stream,
        Action<ChatCompletionRequest>? requestModifier)
    {
        ArgumentNullException.ThrowIfNull(messages);
        maxTokens ??= GetDefaultMaxTokens(model);
        var request = new ChatCompletionRequest(messages)
        {
            Model = model,
            MaxTokens = maxTokens,
            Stream = stream,
            User = user,
            Temperature = temperature,
            ResponseFormat = jsonMode ? new ChatCompletionRequest.ChatCompletionResponseFormat(jsonMode) : null,
            Seed = seed,
        };
        requestModifier?.Invoke(request);
        return request;
    }

    public int? GetDefaultMaxTokens(string model)
    {
        return null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamChatCompletions(
        UserOrSystemMessage messages,
        int? maxTokens = null,
        string model = ChatCompletionModels.Default,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        bool jsonMode = false,
        long? seed = null,
        Action<ChatCompletionRequest>? requestModifier = null,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        EnsureJsonModeIsSupported(model, jsonMode);
        ThrowIfDisposed();
        var request = CreateChatCompletionRequest(
            messages.GetMessages(),
            maxTokens,
            model,
            temperature,
            user,
            jsonMode,
            seed,
            stream: true,
            requestModifier: requestModifier
        );
        return StreamChatCompletions(request, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request == null) throw new ArgumentNullException(nameof(request));
        EnsureJsonModeIsSupported(request.Model, request.ResponseFormat?.Type == ChatCompletionRequest.ResponseTypes.JsonObject);
        ThrowIfDisposed();
        request.Stream = true;
        await foreach (var response in StreamChatCompletionsRaw(request, cancellationToken))
        {
            var content = response.Choices[0].Delta?.Content;
            if (content is not null)
                yield return content;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsRaw(
        ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureJsonModeIsSupported(request.Model, request.ResponseFormat?.Type == ChatCompletionRequest.ResponseTypes.JsonObject);
        ThrowIfDisposed();
        request.Stream = true;
        return HttpClient.StreamUsingServerSentEvents<ChatCompletionRequest, ChatCompletionResponse>
        (
            ChatCompletionsEndpoint,
            request,
            _nullIgnoreSerializerOptions,
            cancellationToken
        );
    }

    public string GetOptimalModel(ChatCompletionMessage[] messages)
    {
        return ChatCompletionModels.Gpt4Turbo;
    }

    private static void EnsureJsonModeIsSupported(string model, bool jsonMode)
    {
        if(jsonMode && !ChatCompletionModels.IsJsonModeSupported(model))
        {
            throw new NotSupportedException(
                $"Model {model} does not support JSON mode. " +
                $"Supported models are: {string.Join(", ", ChatCompletionModels.GetModelsThatSupportJsonMode())}"
            );
        }
    }
}