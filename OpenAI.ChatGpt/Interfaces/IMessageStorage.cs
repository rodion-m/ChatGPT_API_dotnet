using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Interfaces;

public interface IMessageStorage
{
    /// <summary>
    /// Saves messages for a specified user and topic.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="messages">The messages to save.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveMessages(
        string userId,
        Guid topicId,
        IEnumerable<PersistentChatMessage> messages,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Retrieves messages associated with a user and a topic.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of messages.</returns>
    Task<IEnumerable<PersistentChatMessage>> GetMessages(
        string userId,
        Guid topicId,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Edits the content of a message.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="messageId">The message ID.</param>
    /// <param name="newMessage">The new message content.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EditMessage(string userId, Guid topicId, Guid messageId, string newMessage,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a message associated with a user and a topic.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="messageId">The message ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating the success of the deletion.</returns>
    Task<bool> DeleteMessage(string userId, Guid topicId, Guid messageId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Saves a user or system message along with an assistant message.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="message">The user or system message.</param>
    /// <param name="assistantMessage">The assistant message.</param>
    /// <param name="dateTime">Date and time for messages.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveMessages(
        string userId,
        Guid topicId,
        UserOrSystemMessage message,
        string assistantMessage,
        DateTimeOffset? dateTime,
        CancellationToken cancellationToken);

    /// <summary>
    /// Generates a new unique message ID.
    /// </summary>
    /// <returns>A new unique message ID.</returns>
    Guid NewMessageId() => Guid.NewGuid();
}