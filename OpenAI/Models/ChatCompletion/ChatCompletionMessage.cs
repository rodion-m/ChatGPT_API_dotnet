using System.Text.Json.Serialization;

namespace OpenAI.Models.ChatCompletion;

public class ChatCompletionMessage
{
    /// <summary>One of <see cref="ChatCompletionRoles"/></summary>
    [JsonPropertyName("role")] 
    public string Role { get; init; }

    /// <summary>The message text</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; }
    
    protected readonly List<ChatCompletionMessage> Messages;
    
    /// <param name="role">One of <see cref="ChatCompletionRoles"/></param>
    /// <param name="content">The message text</param>
    public ChatCompletionMessage(string role, string content)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(role));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(content));
        Role = role;
        Content = content;
        Messages = new List<ChatCompletionMessage>(1) { this };
    }
    
    internal ChatCompletionMessage(
        List<ChatCompletionMessage> messages,
        string role,
        string content) : this(role, content)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (content == null) throw new ArgumentNullException(nameof(content));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Messages.Add(this);
    }

    public IReadOnlyList<ChatCompletionMessage> GetMessages() => Messages.AsReadOnly();
}