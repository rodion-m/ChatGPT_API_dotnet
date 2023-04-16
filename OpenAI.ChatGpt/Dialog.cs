using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

/// <summary>
/// It's a reccomend way to use this class for creating dialogs.
/// </summary>
public static class Dialog
{
    /// <summary>
    /// Starts a dialog with a user message.
    /// It's a reccomend way to use this method for creating dialogs.
    /// </summary>
    public static UserMessage StartAsUser(string userMessage)
    {
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        return new UserMessage(userMessage);
    }
    
    /// <summary>
    /// Starts a dialog with a system message.
    /// It's a reccomend way to use this method for creating dialogs.
    /// </summary>
    public static SystemMessage StartAsSystem(string systemMessage)
    {
        if (systemMessage == null) throw new ArgumentNullException(nameof(systemMessage));
        return new SystemMessage(systemMessage);
    }
}