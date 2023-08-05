using OpenAI.ChatGpt;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Internal;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGPT.Modules.ChatBot;

internal class ChatGptBot
{
    private readonly string? _initialMessage;
    private readonly int? _maxTokens;
    private readonly string? _model;
    private readonly float? _temperature;
    private readonly ITimeProvider _timeProvider;
    private readonly IChatHistoryStorage _chatHistoryStorage;
    private readonly OpenAiClient _client;

    public ChatGptBot(
        string openAiKey,
        string? initialMessage = null,
        int? maxTokens = null,
        string? model = null,
        float? temperature = null,
        string? host = null,
        IChatHistoryStorage? chatHistoryStorage = null,
        ITimeProvider? timeProvider = null)
    {
        _initialMessage = initialMessage;
        _maxTokens = maxTokens;
        _model = model;
        _temperature = temperature;
        _timeProvider = timeProvider ?? new TimeProviderUtc();
        _chatHistoryStorage = chatHistoryStorage ?? new InMemoryChatHistoryStorage();
        _client = new OpenAiClient(openAiKey, host);
    }
    
    public async Task<string> GetResponse(string message, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(userId);
        var chatGpt = new ChatGpt.ChatGPT(_client, _chatHistoryStorage, _timeProvider, userId, new ChatGPTConfig()
        {
            InitialSystemMessage = _initialMessage,
            MaxTokens = _maxTokens,
            Model = _model,
            Temperature = _temperature
        });
        
        var service = await chatGpt.ContinueOrStartNewTopic(cancellationToken);
        //var messages = await service.GetMessages(cancellationToken);
        //messages.First().CalculateApproxTotalTokenCount();
        var response = await service.GetNextMessageResponse(message, cancellationToken);
        return response;
    }
}