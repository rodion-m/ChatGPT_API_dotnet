using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Interfaces;

/// <summary>
/// Represents a storage for managing messages and topics.
/// </summary>
public interface IChatHistoryStorage : ITopicStorage, IMessageStorage
{
    /// <summary>
    /// Ensures that the storage required for the message store is created.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnsureStorageCreated(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    /// <returns>The current date and time.</returns>
    DateTimeOffset Now() => DateTimeOffset.Now;

    /// <inheritdoc />
    Task IMessageStorage.SaveMessages(
        string userId,
        Guid topicId,
        UserOrSystemMessage message,
        string assistantMessage,
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        var now = Now();
        var enumerable = new PersistentChatMessage[]
        {
            new(NewMessageId(), userId, topicId, now, message),
            new(NewMessageId(), userId, topicId, now, ChatCompletionRoles.Assistant,
                assistantMessage),
        };
        return SaveMessages(userId, topicId, enumerable, cancellationToken);
    }
}