namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class ChatHistoryCacheConfig
{
    public TimeSpan? MessagesSlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan? TopicsSlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);
}