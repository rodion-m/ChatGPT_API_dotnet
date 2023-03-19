using System.Net;

namespace OpenAI.Exceptions;

public class ServerSentEventsResponseException : NotExpectedResponseException
{
    internal ServerSentEventsResponseException(HttpStatusCode statusCode, string content) 
        : base(
            $"Server sent events request returned status code {statusCode}: {content}",
            statusCode, 
            content)
    {
    }
}