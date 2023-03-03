using OpenAI.Models.ChatCompletion;

namespace OpenAI;

public static class Dialog
{
    public static UserMessage StartAsUser(string userMessage)
    {
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        return new ChatCompletionDialog(userMessage);
    }
    
    public static SystemMessage StartAsSystem(string systemMessage)
    {
        if (systemMessage == null) throw new ArgumentNullException(nameof(systemMessage));
        return new SystemMessage(systemMessage);
    }
}