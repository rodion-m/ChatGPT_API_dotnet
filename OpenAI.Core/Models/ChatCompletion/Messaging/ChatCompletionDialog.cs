namespace OpenAI.Models.ChatCompletion;

public class ChatCompletionDialog : UserMessage
{
    public ChatCompletionDialog(string userMessage) : base(userMessage)
    {
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
    }
    
    internal ChatCompletionDialog(List<ChatCompletionMessage> messages, string userMessage) 
        : base(messages, userMessage)
    {
    }
}