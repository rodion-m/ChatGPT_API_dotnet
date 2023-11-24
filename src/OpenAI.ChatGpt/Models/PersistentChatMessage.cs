using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt.Models;

public class PersistentChatMessage(Guid id,
        string userId,
        Guid topicId,
        DateTimeOffset createdAt,
        string role,
        string content)
    : ChatCompletionMessage(role, content)
{
    public Guid Id { get; init; } = id;
    public string UserId { get; set; } = userId ?? throw new ArgumentNullException(nameof(userId));
    public Guid TopicId { get; set; } = topicId;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;

    public PersistentChatMessage(
        Guid id,
        string userId,
        Guid topicId,
        DateTimeOffset createdAt,
        ChatCompletionMessage message) 
        : this(
            id, 
            userId ?? throw new ArgumentNullException(nameof(userId)), 
            topicId, 
            createdAt, 
            message.Role, 
            message.Content)
    {
    }
}