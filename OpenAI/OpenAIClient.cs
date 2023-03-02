using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Models.ChatCompletion;
using OpenAI.Models.Images;

namespace OpenAI;

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

    public OpenAiClient(string apiKey)
    {
        var apiKey1 = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(DefaultHost)
        };
        var header = new AuthenticationHeaderValue("Bearer", apiKey1);
        _httpClient.DefaultRequestHeaders.Authorization = header;
    }

    public void Dispose() => _httpClient.Dispose();

    public async Task<string> GetChatCompletions(
        UserMessage messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Gpt35Turbo,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        var res = await GetChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages.GetDialog()
        }, cancellationToken);
        return res.Choices[0].Message!.Content;
    }

    public async Task<string> GetChatCompletions(
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Gpt35Turbo,
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

    public async Task<ChatCompletionResponse> GetChatCompletions(
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
            ThrowChatCompletionResponseException(response.StatusCode, responseContent);
        }

        var jsonResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent)!;
        return jsonResponse;
    }

    private static void ThrowChatCompletionResponseException(
        HttpStatusCode statusCode,
        string responseContent)
    {
        throw new Exception($"Failed to generate chat response *{statusCode}:" +
                            $" {responseContent}");
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
        IEnumerable<ChatCompletionMessage> messages,
        int maxTokens = ChatCompletionRequest.MaxTokensDefault,
        string model = ChatCompletionModels.Gpt35Turbo,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        return StreamChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages,
            Stream = true
        }, cancellationToken);
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
        string model = ChatCompletionModels.Gpt35Turbo,
        CancellationToken cancellationToken = default)
    {
        if (messages == null) throw new ArgumentNullException(nameof(messages));
        if (model == null) throw new ArgumentNullException(nameof(model));
        return StreamChatCompletions(new ChatCompletionRequest()
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = messages.GetDialog(),
            Stream = true
        }, cancellationToken);
    }

    public async IAsyncEnumerable<string> StreamChatCompletions(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        request.Stream = true;
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsEndpoint)
        {
            Content = JsonContent.Create(request, options: _nullIgnoreSerializerOptions)
        };
        var response = await _httpClient
            .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
            ThrowChatCompletionResponseException(response.StatusCode, responseContent);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            if (line.StartsWith("data: "))
                line = line.Substring("data: ".Length);

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line == "[DONE]")
            {
                yield break;
            }

            var res = JsonSerializer.Deserialize<ChatCompletionResponse>(line);
            var content = res!.Choices[0].Delta?.Content;
            if (content is not null)
                yield return content;
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