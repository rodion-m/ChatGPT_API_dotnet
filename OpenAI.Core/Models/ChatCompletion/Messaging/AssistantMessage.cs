namespace OpenAI.Models.ChatCompletion;

/// <summary>
/// A message from the assistant (ChatGPT)
/// </summary>
/// <remarks>
/// The assistant messages help store prior responses.
/// They can also be written by a developer to help give examples of desired behavior.
/// </remarks>
public class AssistantMessage : ChatCompletionMessage
{
    /// <summary>
    /// A message from the assistant (ChatGPT)
    /// </summary>
    /// <remarks>
    /// The assistant messages help store prior responses.
    /// They can also be written by a developer to help give examples of desired behavior.
    /// </remarks>
    /// <param name="content">The message text</param>
    public AssistantMessage(string content) : base(ChatCompletionRoles.Assistant, content)
    {
    }
    
    internal AssistantMessage(List<ChatCompletionMessage> messages, string content) 
        : base(messages, ChatCompletionRoles.Assistant, content)
    {
    }

    public UserMessage ThenUser(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userMessage));
        return new UserMessage(Messages, userMessage);
    }
}