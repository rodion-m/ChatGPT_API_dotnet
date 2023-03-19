namespace OpenAI.ChatCompletions.Chat.Models;

public class Topic
{
#pragma warning disable CS8618
    private Topic() {}
#pragma warning restore CS8618


    public Topic(
        Guid id, 
        string userId, 
        string? name, 
        DateTimeOffset createdAt,
        ChatCompletionsConfig config)
    {
        Id = id;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Name = name;
        CreatedAt = createdAt;
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ChatCompletionsConfig Config { get; set; }
}