namespace OpenAI.ChatGpt.Internal;

public interface ITimeProvider
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    /// <returns>The current date and time.</returns>
    DateTimeOffset GetCurrentTime();
}