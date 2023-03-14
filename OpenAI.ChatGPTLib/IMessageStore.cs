using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

public interface IMessageStore
{
    Task SaveMessages(string userId, Guid chatId, IEnumerable<ChatCompletionMessage> messages, CancellationToken cancellationToken);
    Task<IEnumerable<ChatCompletionMessage>> GetMessages(string userId, Guid chatId, CancellationToken cancellationToken);
    
    Task<IEnumerable<ChatInfo>> GetChats(string userId, CancellationToken cancellationToken);

    Task SaveMessages(
        string userId, Guid chatId, 
        string userMessage, string assistantMessage, CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (userMessage == null) throw new ArgumentNullException(nameof(userMessage));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var enumerable =  new ChatCompletionMessage[]
        {
            new UserMessage(userMessage),
            new AssistantMessage(assistantMessage)
        };
        return SaveMessages(userId, chatId, enumerable, cancellationToken);
    }
    
    Task<ChatInfo> GetChatInfo(string userId, Guid chatId, CancellationToken cancellationToken);
    Task AddChatInfo(string userId, ChatInfo chatInfo, CancellationToken cancellationToken);
    
    //Task SetCurrentChatId(string userId, Guid chatId, CancellationToken cancellationToken);
// #pragma warning disable CA1822
//     internal Task<Guid?> GetLastChatIdOrNull(string userId, CancellationToken cancellationToken)
// #pragma warning restore CA1822
//     {
//         throw new NotImplementedException("This method is reserved to the future");
//     }

}