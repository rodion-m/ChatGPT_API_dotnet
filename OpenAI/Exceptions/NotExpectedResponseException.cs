using System.Net;

namespace OpenAI.Exceptions;

internal class NotExpectedResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string Content { get; }

    public NotExpectedResponseException(
        HttpStatusCode statusCode,
        string content) : base($"Failed to retrive response ({statusCode}): {content}")
    {
        StatusCode = statusCode;
        Content = content;
    }
    
    public NotExpectedResponseException(
        string message,
        HttpStatusCode statusCode,
        string content) : base(message)
    {
        StatusCode = statusCode;
        Content = content;
    }
}