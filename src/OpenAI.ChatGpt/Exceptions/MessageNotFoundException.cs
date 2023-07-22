namespace OpenAI.ChatGpt.Exceptions;

public class MessageNotFoundException : Exception
{
    public Guid MessageId { get; }

    public MessageNotFoundException(Guid messageId) : base($"Message {messageId} not found.")
    {
        MessageId = messageId;
    }
}