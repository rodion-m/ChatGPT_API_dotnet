using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace OpenAI.ChatGpt.Models.Images;

internal class ImagesGenerationUriResponse
{
    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("data")]
    public Datum[] Data { get; set; }
    
    public class Datum
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; } = null!;
    }
}
