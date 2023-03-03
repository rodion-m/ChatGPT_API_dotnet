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

/// <summary> A message from the user </summary>
public class UserMessage : ChatCompletionMessage
{
    /// <summary> A message from the user </summary>
    /// <param name="content">The message text</param>
    public UserMessage(string content) : base(ChatCompletionRoles.User, content)
    {
    }
    
    internal UserMessage(List<ChatCompletionMessage> messages, string content) 
        : base(messages, ChatCompletionRoles.User, content)
    {
    }

    public AssistantMessage ThenAssistant(string assistantMessage)
    {
        if (string.IsNullOrWhiteSpace(assistantMessage))
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(assistantMessage));
        return new AssistantMessage(Messages, assistantMessage);
    }
    
    public static explicit operator UserMessage(string message)
    {
        return new UserMessage(message);
    }
}

/// <summary> A message from the assistant (ChatGPT) </summary>
public class AssistantMessage : ChatCompletionMessage
{
    /// <summary> A message from the assistant (ChatGPT) </summary>
    /// <param name="content">The message text</param>
    public AssistantMessage(string content) : base(ChatCompletionRoles.Assistant, content)
    {
    }
    
    internal AssistantMessage(List<ChatCompletionMessage> messages, string content) 
        : base(messages, ChatCompletionRoles.Assistant, content)
    {
    }

    public ChatCompletionDialog ThenUser(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userMessage));
        return new ChatCompletionDialog(Messages, userMessage);
    }
}

/// <summary> A message from the assistant (ChatGPT) </summary>
public class SystemMessage : ChatCompletionMessage
{
    /// <summary> A message from the assistant (ChatGPT) </summary>
    /// <param name="content">The message text</param>
    public SystemMessage(string content) : base(ChatCompletionRoles.System, content)
    {
    }
    
    internal SystemMessage(List<ChatCompletionMessage> messages, string content) 
        : base(messages, ChatCompletionRoles.Assistant, content)
    {
    }

    public ChatCompletionDialog ThenUser(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userMessage));
        return new ChatCompletionDialog(Messages, userMessage);
    }
}

public class ChatCompletionDialog : UserMessage
{
    public ChatCompletionDialog(string userMessage) : base(userMessage)
    {
    }
    
    internal ChatCompletionDialog(List<ChatCompletionMessage> messages, string userMessage) 
        : base(messages, userMessage)
    {
    }
}