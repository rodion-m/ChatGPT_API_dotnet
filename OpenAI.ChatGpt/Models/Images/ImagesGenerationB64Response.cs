using System.Text.Json.Serialization;

namespace OpenAI.ChatGpt.Models.Images;

internal class ImagesGenerationB64Response
{
    [JsonPropertyName("created")] public long Created { get; set; }

    [JsonPropertyName("data")] public Datum[] Data { get; set; } = null!;

    public class Datum
    {
        [JsonPropertyName("b64_json")] public string B64Json { get; set; } = null!;
    }
}

