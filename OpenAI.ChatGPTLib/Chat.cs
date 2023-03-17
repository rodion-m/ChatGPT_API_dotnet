using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.ChatCompletions.Chat.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat;

/// <summary>
/// This is a class that is used for communication between a user and the assistant (ChatGPT).
/// </summary>
/// <remarks>Not thread-safe. Use one instance per user.</remarks>
public class Chat : IDisposable
{
    public ChatInfo ChatInfo { get; }
    public string UserId { get; }
    public Guid ChatId => ChatInfo.Id;
    public bool IsWriting { get; private set; }
    public bool IsCancelled => _cts?.IsCancellationRequested ?? false;

    private readonly IMessageStore _messageStore;
    private readonly OpenAiClient _client;
    private bool _isNew;
    private CancellationTokenSource? _cts;

    internal Chat(
        IMessageStore messageStore,
        OpenAiClient client,
        string userId,
        ChatInfo chatInfo,
        bool isNew)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        ChatInfo = chatInfo ?? throw new ArgumentNullException(nameof(chatInfo));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _isNew = isNew;
    }
    
    public async IAsyncEnumerable<string> StreamNextMessageResponse(
        string message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cts.Token.Register(() => IsWriting = false);
        
        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(new UserMessage(message));
        //todo calculate chat length and check against max tokens
        var sb = new StringBuilder();
        var stream = _client.StreamChatCompletions(
            messages,
            user: ChatInfo.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: ChatInfo.Config.ModifyRequest,
            cancellationToken: _cts.Token
        );
        IsWriting = true;
        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            sb.Append(chunk);
            yield return chunk;
        }
        
        await _messageStore.SaveMessages(
            UserId, ChatId, message, sb.ToString(), _cts.Token);
        IsWriting = false;
        _isNew = false;
    }

    private Task<IEnumerable<ChatCompletionMessage>> LoadHistory(CancellationToken cancellationToken)
    {
        if (_isNew) return Task.FromResult(Enumerable.Empty<ChatCompletionMessage>());
        return _messageStore.GetMessages(UserId, ChatId, cancellationToken);
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}