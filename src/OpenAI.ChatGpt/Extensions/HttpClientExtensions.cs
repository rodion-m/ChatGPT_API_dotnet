﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenAI.ChatGpt.Exceptions;

namespace OpenAI.ChatGpt.Extensions;

[Fody.ConfigureAwait(false)]
internal static class HttpClientExtensions
{
    private static readonly int DataHeaderLength = "data: ".Length;
        
    private enum ProcessResponseEventResult
    {
        Response,
        Done,
        Empty
    }

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
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(requestUri);
        ArgumentNullException.ThrowIfNull(request);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(request, options: serializerOptions)
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        using var response = await SendAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ServerSentEventsResponseException(response.StatusCode, responseContent);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var reader = new StreamReader(stream);
        while (await ReadLineAsync(reader, cancellationToken) is { } line)
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

        yield break;


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
        ArgumentNullException.ThrowIfNull(reader);
#if NET7_0_OR_GREATER
        return reader.ReadLineAsync(cancellationToken);
#else
        return new ValueTask<string?>(reader.ReadLineAsync());
#endif
    }
}