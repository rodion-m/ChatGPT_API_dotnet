using System.Text.Json.Serialization;

namespace OpenAI.Models.ChatCompletion;

/// <summary>
/// A message in a chat dialog.
/// It's recommended to use <see cref="Dialog"/> bulder to create conversation.
/// </summary>
public class ChatCompletionMessage
{
    /// <summary>
    /// The role of the message in the chat.
    /// One of <see cref="ChatCompletionRoles"/>
    /// </summary>
    /// <remarks>
    /// See https://github.com/openai/openai-python/blob/main/chatml.md for more information.
    /// </remarks>
    [JsonPropertyName("role")]
    public string Role { get; init; }

    /// <summary>The message text</summary>
    [JsonPropertyName("content")]
    public string Content { get; init; }
    
    protected readonly List<ChatCompletionMessage> Messages;
    
    /// <param name="role">One of <see cref="ChatCompletionRoles"/></param>
    /// <param name="content">The message text</param>
    public ChatCompletionMessage(string role, string content)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(role));
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(content));
        Role = role;
        Content = content;
        Messages = new List<ChatCompletionMessage>(1) { this };
    }
    
    internal ChatCompletionMessage(
        List<ChatCompletionMessage> messages,
        string role,
        string content) : this(role, content)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (content == null) throw new ArgumentNullException(nameof(content));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        Messages.Add(this);
    }

    public IReadOnlyList<ChatCompletionMessage> GetMessages() => Messages.AsReadOnly();
    
    /// <summary>
    /// Calculates the approximate total token count of all messages.
    /// It's can be used to determine if the dialog messages is too long.
    /// Each model max tokens is limited by <see cref="ChatCompletionModels.GetMaxTokensLimitForModel"/>.
    /// </summary>
    /// <remarks>
    /// Rule of thumb is that one token generally corresponds to ~4 characters of text for common English text
    /// See: https://platform.openai.com/tokenizer
    /// </remarks>
    public int CalculateApproxTotalTokenCount() => CalculateApproxTotalTokenCount(Messages);
    
    private const double AverageCharactersCountInOneToken = 4d;

    /// <summary>
    /// Calculates the approximate total token count of all messages.
    /// It's can be used to determine if the dialog messages is too long.
    /// Each model max tokens is limited by <see cref="ChatCompletionModels.GetMaxTokensLimitForModel"/>.
    /// </summary>
    /// <remarks>
    /// Rule of thumb is that one token generally corresponds to ~4 characters of text for common English text
    /// See: https://platform.openai.com/tokenizer
    /// </remarks>
    public static int CalculateApproxTotalTokenCount(IEnumerable<ChatCompletionMessage> messages)
    {
        var dialogCharsCount = messages.Sum(m => m.Content.Length);
        return (int)(dialogCharsCount / AverageCharactersCountInOneToken);
    }


    /// <summary> How many tokens in one word (0.75) </summary>
    private const double TokenToWordAverageRatio = 3d / 4d;
    private const double TokensAverageQuantityInOneWord = 1d / TokenToWordAverageRatio;
}