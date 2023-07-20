using System.Text.Json;

namespace OpenAI.ChatGpt.Modules.StructuredResponse;

public class InvalidJsonException : Exception
{
    public InvalidJsonException(string message, string response, JsonException? jsonException = null) 
        : base(message, jsonException)
    {
        Response = response ?? throw new ArgumentNullException(nameof(response));
        JsonException = jsonException;
    }

    public JsonException? JsonException { get; }

    public string Response { get; }
}