namespace OpenAI.ChatGpt.Internal;

public class TimeProviderUtc : ITimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset GetCurrentTime() => DateTimeOffset.UtcNow;
}