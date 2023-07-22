using OpenAI.ChatGpt.Models;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;
using static OpenAI.ChatGpt.Models.ChatCompletion.ChatCompletionRoles;

namespace OpenAI.ChatGpt.Interfaces;

/// <summary>
/// Represents a storage for managing messages and topics.
/// </summary>
public interface IChatHistoryStorage : ITopicStorage, IMessageStorage
{
    /// <summary>
    /// Ensures that the storage required for the chat history store is created.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EnsureStorageCreated(CancellationToken cancellationToken);

    /// <inheritdoc />
    Task IMessageStorage.SaveMessages(
        string userId,
        Guid topicId,
        UserOrSystemMessage message,
        string assistantMessage,
        DateTimeOffset? dateTime,
        CancellationToken cancellationToken)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (assistantMessage == null) throw new ArgumentNullException(nameof(assistantMessage));
        dateTime ??= DateTimeOffset.UtcNow;
        var enumerable = new PersistentChatMessage[]
        {
            new(NewMessageId(), userId, topicId, dateTime.Value, message),
            new(NewMessageId(), userId, topicId, dateTime.Value, Assistant, assistantMessage),
        };
        return SaveMessages(userId, topicId, enumerable, cancellationToken);
    }
}