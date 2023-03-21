using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt;

public interface IMessageStore
{
    Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken);
    Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken);
    Task AddTopic(string userId, Topic topic, CancellationToken cancellationToken);

    Task SaveMessages(
        string userId,
        Guid topicId,
        IEnumerable<ChatCompletionMessage> messages,
        CancellationToken cancellationToken
    );

    Task<IEnumerable<ChatCompletionMessage>> GetMessages(
        string userId, 
        Guid topicId,
        CancellationToken cancellationToken
    );
    
    Task<Topic?> GetLastTopicOrNull(string userId, CancellationToken cancellationToken);

    Task SaveMessages(string userId,
        Guid topicId,
        UserOrSystemMessage message,
        string assistantMessage,
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var enumerable = new ChatCompletionMessage[]
        {
            message,
            new AssistantMessage(assistantMessage)
        };
        return SaveMessages(userId, topicId, enumerable, cancellationToken);
    }

    Guid NewTopicId() => Guid.NewGuid();
}