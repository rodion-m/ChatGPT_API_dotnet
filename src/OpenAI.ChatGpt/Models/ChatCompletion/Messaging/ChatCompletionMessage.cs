using System.Text.Json.Serialization;

namespace OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

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
    [JsonPropertyName("content"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Content { get; set; }

    private List<ChatCompletionMessage>? _messages;

    protected List<ChatCompletionMessage> Messages
    {
        get { return _messages ??= new List<ChatCompletionMessage>() { this }; }
    }

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
    }

    internal ChatCompletionMessage(
        List<ChatCompletionMessage> messages,
        string role,
        string content) : this(role, content)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (content == null) throw new ArgumentNullException(nameof(content));
        _messages = messages ?? throw new ArgumentNullException(nameof(messages));
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
    public static int CalculateApproxTotalTokenCount(IEnumerable<ChatCompletionMessage> messages, bool? isEnglish = null)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        var dialogCharsCount = messages.Sum(m => m.Content.Length);
        if (dialogCharsCount == 0)
        {
            return 0;
        }
        // ReSharper disable once PossibleMultipleEnumeration
        return CalculateApproxTotalTokenCount(dialogCharsCount, isEnglish ?? IsEnglish(messages.First().Content));
    }

    private static bool IsEnglish(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var englishCharsCount = content.Count(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z');
        return (double) englishCharsCount / content.Length >= 0.8;
    }

    public static int CalculateApproxTotalTokenCount(int totalCharsCount, bool isEnglish)
    {
        return isEnglish
            ? (int)(totalCharsCount / AverageCharactersCountInOneToken)
            : totalCharsCount;
    }

    /// <summary> How many tokens in one word (0.75) </summary>
    private const double TokenToWordAverageRatio = 3d / 4d;

    private const double TokensAverageQuantityInOneWord = 1d / TokenToWordAverageRatio;

    public override string ToString()
    {
        return _messages == null
            ? $"{Role}: {Content}"
            : string.Join(Environment.NewLine, _messages.Select(m => $"{m.Role}: {m.Content}"));
    }
}