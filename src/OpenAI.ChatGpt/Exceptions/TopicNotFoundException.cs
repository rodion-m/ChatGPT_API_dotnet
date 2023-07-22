namespace OpenAI.ChatGpt.Exceptions;

/// <summary> Exception thrown when a topic is not found. </summary>
public class TopicNotFoundException : Exception
{
    public Guid TopicId { get; }

    public TopicNotFoundException(Guid topicId) : base($"Topic {topicId} not found.")
    {
        TopicId = topicId;
    }
}