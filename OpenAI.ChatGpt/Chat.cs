using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Internal;
using OpenAI.ChatGpt.Models;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

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

    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly OpenAiClient _client;
    private bool _isNew;
    private CancellationTokenSource? _cts;

    internal Chat(
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        OpenAiClient client,
        string userId,
        Topic topic,
        bool isNew)
    {
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
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

        await _chatHistoryStorage.SaveMessages(
            UserId, ChatId, message, response, _clock.GetCurrentTime(), _cts.Token);
        IsWriting = false;
        _isNew = false;

        return response;
    }
    
    public IAsyncEnumerable<string> StreamNextMessageResponse(
        string message,
        bool throwOnCancellation = true,
        CancellationToken cancellationToken = default)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        var chatCompletionMessage = new UserMessage(message);
        return StreamNextMessageResponse(chatCompletionMessage, throwOnCancellation, cancellationToken);
    }

    private async IAsyncEnumerable<string> StreamNextMessageResponse(
        UserOrSystemMessage message, 
        bool throwOnCancellation,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var originalCancellationToken = cancellationToken;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(originalCancellationToken);
        cancellationToken = _cts.Token;
        cancellationToken.Register(() => IsWriting = false);

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message);
        var sb = new StringBuilder();
        IsWriting = true;
        var stream = _client.StreamChatCompletions(
            messages,
            user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: Topic.Config.ModifyRequest,
            cancellationToken: cancellationToken
        );
        await foreach (var chunk in stream
                           .ConfigureExceptions(throwOnCancellation)
                           .WithCancellation(cancellationToken))
        {
            sb.Append(chunk);
            yield return chunk;
        }
        
        if(cancellationToken.IsCancellationRequested && !throwOnCancellation)
            yield break;

        await _chatHistoryStorage.SaveMessages(
            UserId, ChatId, message, sb.ToString(), _clock.GetCurrentTime(), cancellationToken);
        IsWriting = false;
        _isNew = false;
    }

    private async Task<IEnumerable<ChatCompletionMessage>> LoadHistory(CancellationToken cancellationToken)
    {
        if (_isNew) return Enumerable.Empty<ChatCompletionMessage>();
        return await _chatHistoryStorage.GetMessages(UserId, ChatId, cancellationToken);
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