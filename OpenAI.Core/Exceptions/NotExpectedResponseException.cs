using System.Net;

namespace OpenAI.Exceptions;

public class NotExpectedResponseException : Exception
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public HttpStatusCode StatusCode { get; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Content { get; }

    internal NotExpectedResponseException(
        HttpStatusCode statusCode,
        string content) : base($"Failed to retrive response ({statusCode}): {content}")
    {
        StatusCode = statusCode;
        Content = content;
    }
    
    internal NotExpectedResponseException(
        string message,
        HttpStatusCode statusCode,
        string content) : base(message)
    {
        StatusCode = statusCode;
        Content = content;
    }
}