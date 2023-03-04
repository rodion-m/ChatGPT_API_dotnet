namespace OpenAI.Models.ChatCompletion;

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