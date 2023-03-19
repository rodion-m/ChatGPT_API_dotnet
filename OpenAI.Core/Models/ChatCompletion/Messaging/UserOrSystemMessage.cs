namespace OpenAI.Models.ChatCompletion;

/// <summary>
/// Common base class for <see cref="UserMessage"/>> and <see cref="SystemMessage"/>
/// </summary>
public abstract class UserOrSystemMessage : ChatCompletionMessage
{
    protected UserOrSystemMessage(string role, string content) : base(role, content)
    {
    }

    protected UserOrSystemMessage(List<ChatCompletionMessage> messages, string role, string content) 
        : base(messages, role, content)
    {
    }
}