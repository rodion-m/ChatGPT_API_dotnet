using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OpenAI;

internal static class HttpClientExtensions
{
    private static readonly int DataHeaderLength = "data: ".Length;

    // https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events
    internal static async IAsyncEnumerable<TResponse>
        StreamUsingServerSentEvents<TRequest, TResponse>(
            this HttpClient httpClient,
            string requestUri,
            TRequest request,
            JsonSerializerOptions? serializerOptions = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRequest : notnull
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request, options: serializerOptions)
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        using var response = await SendAsync().ConfigureAwait(false);
        
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);
            throw new ServerSentEventsResponseException(response.StatusCode, responseContent);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        var reader = new StreamReader(stream);
        while (await ReadLineAsync(reader, cancellationToken).ConfigureAwait(false) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (result, data) = ProcessResponseEvent(line);
            switch (result)
            {
                case ProcessResponseEventResult.Done:
                    yield break;
                case ProcessResponseEventResult.Empty:
                    continue;
                case ProcessResponseEventResult.Response:
                    yield return data!;
                    break;
            }
        }


        Task<HttpResponseMessage> SendAsync()
        {
            return httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }

        (ProcessResponseEventResult result, TResponse? data) ProcessResponseEvent(string line)
        {
            if (line.StartsWith("data: "))
            {
                line = line[DataHeaderLength..];
            }

            if (string.IsNullOrWhiteSpace(line)) return (ProcessResponseEventResult.Empty, default);

            if (line == "[DONE]")
            {
                return (ProcessResponseEventResult.Done, default);
            }

            var data = JsonSerializer.Deserialize<TResponse>(line);
            if (data is null)
            {
                throw new JsonException(
                    $"Failed to deserialize response: {line} to type {typeof(TResponse)}");
            }

            return (ProcessResponseEventResult.Response, data);
        }
    }

    private static ValueTask<string?> ReadLineAsync(
        TextReader reader,
        CancellationToken cancellationToken)
    {
#if NET7_0_OR_GREATER
        return reader.ReadLineAsync(cancellationToken);
#else
        return new ValueTask<string?>(reader.ReadLineAsync());
#endif
    }
}

public enum ProcessResponseEventResult
{
    Response,
    Done,
    Empty
}

public class ServerSentEventsResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ResponseContent { get; }

    internal ServerSentEventsResponseException(HttpStatusCode statusCode, string responseContent)
        : base($"Server sent events request returned status code {statusCode}: {responseContent}")
    {
        StatusCode = statusCode;
        ResponseContent =
            responseContent ?? throw new ArgumentNullException(nameof(responseContent));
    }
}