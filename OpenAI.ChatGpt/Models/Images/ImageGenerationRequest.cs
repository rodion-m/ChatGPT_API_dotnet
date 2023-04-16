using System.Text.Json.Serialization;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace OpenAI.ChatGpt.Models.Images;

internal record ImageGenerationRequest(
    string Prompt,
    string Size,
    [property: JsonPropertyName("n")] int N,
    string ResponseFormat,
    [property: JsonPropertyName("user")] string? User)
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; init; } = Prompt ?? throw new ArgumentNullException(nameof(Prompt));

    [JsonPropertyName("size")]
    public string Size { get; init; } = Size ?? throw new ArgumentNullException(nameof(Size));

    [JsonPropertyName("response_format")]
    public string ResponseFormat { get; init; } 
        = ResponseFormat ?? throw new ArgumentNullException(nameof(ResponseFormat));
}