using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618
namespace OpenAI.ChatGpt.Models.ChatCompletion;

public class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    /// <summary>
    /// The Unix timestamp (in seconds) of when the chat completion was created.
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; set; }
    
    [JsonPropertyName("model")]
    public string Model { get; set; }

    /// <summary>
    /// This fingerprint represents the backend configuration that the model runs with.
    /// Can be used in conjunction with the <see cref="ChatCompletionRequest.Seed"/> request parameter to understand when backend changes have been made that might impact determinism.
    /// </summary>
    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; }

    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; }

    [JsonPropertyName("usage")]
    public _Usage Usage { get; set; }
    
    
    public class Choice
    {
        [JsonPropertyName("index")]
        public long Index { get; set; }
        
        [JsonPropertyName("delta")]
        public Delta? Delta { get; set; }

        [JsonPropertyName("message")]
        public ChatCompletionMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        public bool FinishedByStop => FinishReason == "stop";
        public bool FinishedByLength => FinishReason == "length";
    }
    
    public class Delta
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public class _Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public long PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public long CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }
    }

    public string GetMessageContent()
    {
        if(Choices.Length == 0)
        {
            throw new InvalidOperationException("Choices is empty");
        }
        if (Choices[0].Message == null)
        {
            throw new InvalidOperationException("Message is null");
        }
        return Choices[0].Message!.Content;
    }
}
