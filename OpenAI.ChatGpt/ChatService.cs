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
[Fody.ConfigureAwait(false)]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ChatService : IDisposable, IAsyncDisposable
{
    public Topic Topic { get; }
    public string UserId { get; }
    public Guid TopicId => Topic.Id;
    public bool IsWriting { get; private set; }
    public bool IsCancelled => _cts?.IsCancellationRequested ?? false;
    
    public ChatCompletionResponse? LastResponse { get; private set; }

    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly ITimeProvider _clock;
    private readonly IOpenAiClient _client;
    private readonly bool _clearOnDisposal;
    private CancellationTokenSource? _cts;
    private bool _isNew;

    internal ChatService(
        IChatHistoryStorage chatHistoryStorage,
        ITimeProvider clock,
        IOpenAiClient client,
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
        if (IsWriting)
        {
            throw new InvalidOperationException(
                "Cannot start a new chat session while the previous one is still in progress.");
        }
        var originalCancellation = cancellationToken;
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(originalCancellation);
        cancellationToken = _cts.Token;

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message).ToArray();
        
        IsWriting = true;
        try
        {
            var (model, maxTokens) = FindOptimalModelAndMaxToken(messages);
            var response = await _client.GetChatCompletionsRaw(
                messages,
                maxTokens: maxTokens,
                model: model,
                user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
                requestModifier: Topic.Config.ModifyRequest,
                cancellationToken: cancellationToken
            );
            SetLastResponse(response);

            var assistantMessage = response.GetMessageContent();
            await _chatHistoryStorage.SaveMessages(
                UserId, TopicId, message, assistantMessage, _clock.GetCurrentTime(), cancellationToken);
            _isNew = false;
            return assistantMessage;
        }
        finally
        {
            IsWriting = false; 
        }
    }

    private (string model, int maxTokens) FindOptimalModelAndMaxToken(ChatCompletionMessage[] messages)
    {
        return ChatCompletionMessage.FindOptimalModelAndMaxToken(
            messages, Topic.Config.Model, Topic.Config.MaxTokens);
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
        if (IsWriting)
        {
            throw new InvalidOperationException("Cannot start a new chat session while the previous one is still in progress.");
        }
        var originalCancellationToken = cancellationToken;
        _cts?.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(originalCancellationToken);
        cancellationToken = _cts.Token;

        var history = await LoadHistory(cancellationToken);
        var messages = history.Append(message).ToArray();
        var sb = new StringBuilder();
        IsWriting = true;
        var (model, maxTokens) = FindOptimalModelAndMaxToken(messages);
        var stream = _client.StreamChatCompletions(
            messages,
            maxTokens: maxTokens,
            model: model,
            user: Topic.Config.PassUserIdToOpenAiRequests is true ? UserId : null,
            requestModifier: Topic.Config.ModifyRequest,
            cancellationToken: cancellationToken
        ).ConfigureExceptions(throwOnCancellation, _ => IsWriting = false);
        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            sb.Append(chunk);
            yield return chunk;
        }
        
        if(cancellationToken.IsCancellationRequested && !throwOnCancellation)
        {
            yield break;
        }
        
        SetLastResponse(null);

        try
        {
            await _chatHistoryStorage.SaveMessages(
                UserId,
                TopicId,
                message,
                sb.ToString(),
                _clock.GetCurrentTime(),
                cancellationToken);
            _isNew = false;
        }
        finally
        {
            IsWriting = false;
        }
    }

    private async Task<IEnumerable<ChatCompletionMessage>> LoadHistory(
        CancellationToken cancellationToken)
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