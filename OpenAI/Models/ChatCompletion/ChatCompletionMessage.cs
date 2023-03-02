using System.Text.Json.Serialization;

namespace OpenAI.Models.ChatCompletion;

public class ChatCompletionMessage
{
    protected readonly List<ChatCompletionMessage> Dialog;
    
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
        Dialog = new List<ChatCompletionMessage>(1) { this };
    }
    
    internal ChatCompletionMessage(
        List<ChatCompletionMessage> messages,
        string role,
        string content) : this(role, content)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (content == null) throw new ArgumentNullException(nameof(content));
        Dialog = messages ?? throw new ArgumentNullException(nameof(messages));
        Dialog.Add(this);
    }

    /// <summary>One of <see cref="ChatCompletionRoles"/></summary>
    [JsonPropertyName("role")] 
    public string Role { get; init; }

    /// <summary>The message text</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; }

    public IReadOnlyList<ChatCompletionMessage> GetDialog() => Dialog.AsReadOnly();

    protected void Add(ChatCompletionMessage message)
    {
        Dialog.Add(message);
    }
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
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        return new AssistantMessage(Dialog, assistantMessage);
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
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        return new ChatCompletionDialog(Dialog, userMessage);
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