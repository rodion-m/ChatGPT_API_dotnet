using OpenAI.ChatGpt.Models;
using OpenAI.ChatGpt.Exceptions;

namespace OpenAI.ChatGpt.Interfaces;

public interface ITopicStorage
{
    /// <summary>
    /// Retrieves all topics associated with a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of topics.</returns>
    Task<IEnumerable<Topic>> GetTopics(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a topic associated with a user by its ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <exception cref="TopicNotFoundException">Thrown when a topic is not found.</exception>
    /// <returns>A task that represents the asynchronous operation. The task result contains the topic.</returns>
    Task<Topic> GetTopic(string userId, Guid topicId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new topic.
    /// </summary>
    /// <param name="topic">The topic to add.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddTopic(Topic topic, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the last topic associated with a user or null if there are no topics.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the last topic or null.</returns>
    Task<Topic?> GetMostRecentTopicOrNull(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Edits the name of a topic.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="newName">The new topic name.</param>
    /// <param name="cancellationToken">A cancellation token to observe
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EditTopicName(
        string userId, Guid topicId, string newName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a topic associated with a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="topicId">The topic ID.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating the success of the deletion.</returns>
    Task<bool> DeleteTopic(string userId, Guid topicId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Generates a new unique topic ID.
    /// </summary>
    ///
    /// <returns>A new unique topic ID.</returns>
    Guid NewTopicId() => Guid.NewGuid();
}