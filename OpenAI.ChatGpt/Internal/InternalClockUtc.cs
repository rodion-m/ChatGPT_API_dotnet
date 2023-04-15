namespace OpenAI.ChatGpt.Internal;

public class InternalClockUtc : IInternalClock
{
    /// <inheritdoc />
    public DateTimeOffset GetCurrentTime() => DateTimeOffset.UtcNow;
}