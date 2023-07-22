namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class ChatHistoryCacheConfig
{
    private TimeSpan? _messagesSlidingExpiration;
    private TimeSpan? _topicsSlidingExpiration;

    public TimeSpan MessagesSlidingExpiration
    {
        get => _messagesSlidingExpiration ?? TimeSpan.FromMinutes(10);
        set => _messagesSlidingExpiration = value;
    }

    public TimeSpan TopicsSlidingExpiration
    {
        get => _topicsSlidingExpiration ?? TimeSpan.FromMinutes(10);
        set => _topicsSlidingExpiration = value;
    }
}