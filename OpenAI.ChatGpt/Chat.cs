using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.ChatGpt.Models;
using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatGpt;

/// <summary>
/// This is a class that is used for communication between a user and the assistant (ChatGPT).
/// </summary>
/// <remarks>Not thread-safe. Use one instance per user.</remarks>
public class Chat : IDisposable
{
    public Topic Topic { get; }
    public string UserId { get; }
    public Guid ChatId => Topic.Id;
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
        Topic topic,
        bool isNew)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _isNew = isNew;
    }
    
    public Task<string> GetNextMessageResponse(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        var chatCompletionMessage = new UserMessage(message);
        return GetNextMessageResponse(chatCompletionMessage, cancellationToken);
    }

    private async Task<string> GetNextMessageResponse(
        UserOrSystemMessage message, 
         CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cts.Token.Register(() => IsWriting = false);

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message);
        
        IsWriting = true;
        var response = await _client.GetChatCompletions(
            messages,
            user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: Topic.Config.ModifyRequest,
            cancellationToken: _cts.Token
        );

        await _messageStore.SaveMessages(
            UserId, ChatId, message, response, _cts.Token);
        IsWriting = false;
        _isNew = false;

        return response;
    }
    
    public IAsyncEnumerable<string> StreamNextMessageResponse(
        string message,
        CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        var chatCompletionMessage = new UserMessage(message);
        return StreamNextMessageResponse(chatCompletionMessage, cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamNextMessageResponse(
        UserOrSystemMessage message, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cts.Token.Register(() => IsWriting = false);

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message);
        var sb = new StringBuilder();
        IsWriting = true;
        var stream = _client.StreamChatCompletions(
            messages,
            user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: Topic.Config.ModifyRequest,
            cancellationToken: _cts.Token
        );
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