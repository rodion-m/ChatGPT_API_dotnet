using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public interface IMessageStore
{
    Task<IEnumerable<ChatInfo>> GetChats(string userId, CancellationToken cancellationToken);
    Task<ChatInfo> GetChatInfo(string userId, Guid chatId, CancellationToken cancellationToken);
    Task AddChatInfo(string userId, ChatInfo chatInfo, CancellationToken cancellationToken);

    Task SaveMessages(string userId,
        Guid chatId,
        IEnumerable<ChatCompletionMessage> messages,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<ChatCompletionMessage>> GetMessages(
        string userId, 
        Guid chatId,
        CancellationToken cancellationToken
    );
    
    Task SaveMessages(
        string userId, 
        Guid chatId,
        string userMessage, 
        string assistantMessage, 
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var enumerable = new ChatCompletionMessage[]
        {
            new UserMessage(userMessage),
            new AssistantMessage(assistantMessage)
        };
        return SaveMessages(userId, chatId, enumerable, cancellationToken);
    }
}