﻿using System.Text.Json.Serialization;

namespace OpenAI.Models.Images;

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
