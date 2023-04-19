using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Internal;
using OpenAI.ChatGpt.Models;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt;

/// <summary>
/// Used for communication between a user and the assistant (ChatGPT).
/// </summary>
/// <remarks>Not thread-safe. Use one instance per user.</remarks>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class Chat : IDisposable, IAsyncDisposable
{
    public Topic Topic { get; }
    public string UserId { get; }
    public Guid TopicId => Topic.Id;
    public bool IsWriting { get; private set; }
    public bool IsCancelled => _cts?.IsCancellationRequested ?? false;
    
    public ChatCompletionResponse? LastResponse { get; private set; }

    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly OpenAiClient _client;
    private bool _isNew;
    private readonly bool _clearOnDisposal;
    private CancellationTokenSource? _cts;

    internal Chat(
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        OpenAiClient client,
        string userId,
        Topic topic,
        bool isNew,
        bool clearOnDisposal)
    {
        _chatHistoryStorage = chatHistoryStorage ?? throw new ArgumentNullException(nameof(chatHistoryStorage));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        _isNew = isNew;
        _clearOnDisposal = clearOnDisposal;
    }
    
    public void Dispose()
    {
        _cts?.Dispose();
        if (_clearOnDisposal)
        {
            // TODO: log warning about sync disposal
            _chatHistoryStorage.DeleteTopic(UserId, TopicId, default)
                .GetAwaiter().GetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Dispose();
        if (_clearOnDisposal)
        {
            await _chatHistoryStorage.DeleteTopic(UserId, TopicId, default);
        }
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
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _cts.Token.Register(() => IsWriting = false);

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message);
        
        IsWriting = true; //TODO set false on exception
        var response = await _client.GetChatCompletionsRaw(
            messages,
            user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: Topic.Config.ModifyRequest,
            cancellationToken: _cts.Token
        );
        SetLastResponse(response);

        var assistantMessage = response.GetMessageContent();
        await _chatHistoryStorage.SaveMessages(
            UserId, TopicId, message, assistantMessage, _clock.GetCurrentTime(), _cts.Token);
        IsWriting = false;
        _isNew = false;

        return assistantMessage;
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
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(originalCancellationToken);
        cancellationToken = _cts.Token;
        cancellationToken.Register(() => IsWriting = false);

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message);
        var sb = new StringBuilder();
        IsWriting = true; //TODO set false on exception
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
        {
            IsWriting = false;
            yield break;
        }
        
        SetLastResponse(null);

        await _chatHistoryStorage.SaveMessages(
            UserId, TopicId, message, sb.ToString(), _clock.GetCurrentTime(), cancellationToken);
        IsWriting = false;
        _isNew = false;
    }

    private async Task<IEnumerable<ChatCompletionMessage>> LoadHistory(CancellationToken cancellationToken)
    {
        if (_isNew) return Enumerable.Empty<ChatCompletionMessage>();
        return await _chatHistoryStorage.GetMessages(UserId, TopicId, cancellationToken);
    }
    
    
    /// <summary> Returns topic messages history. </summary>
    public Task<IEnumerable<PersistentChatMessage>> GetMessages(
        CancellationToken cancellationToken = default)
    {
        return _chatHistoryStorage.GetMessages(UserId, TopicId, cancellationToken);
    }

    private void SetLastResponse(ChatCompletionResponse? response)
    {
        LastResponse = response;
    }

    public void Stop()
    {
        _cts?.Cancel();
    }
}