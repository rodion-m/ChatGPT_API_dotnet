using Microsoft.Extensions.Options;
using OpenAI.ChatCompletions.Chat.Models;

namespace OpenAI.ChatCompletions.Chat;

public class ChatGPTFactory : IDisposable
{
    private readonly OpenAiClient _client;
    private readonly ChatCompletionsConfig _config;
    private readonly IMessageStore _messageStore;

    public ChatGPTFactory(
        IOptions<ChatGPTConfig> options,
        IMessageStore messageStore)
    {
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _client = new OpenAiClient(options.Value.ApiKey);
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
    }
    
    public ChatGPTFactory(
        string apiKey,
        IMessageStore messageStore,
        ChatCompletionsConfig? config = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        _client = new OpenAiClient(apiKey);
        _config = config ?? new ChatCompletionsConfig();
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
    }
    
    public static ChatGPTFactory CreateInMemory(string apiKey, ChatCompletionsConfig? config = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        return new ChatGPTFactory(apiKey, new InMemoryMessageStore(), config);
    }
    
    public ChatGPT Create(string userId, ChatCompletionsConfig? config = null)
    {
        if (userId == null) throw new ArgumentNullException(nameof(userId));
        // one of config or _config must be not null:
        return new ChatGPT(
            _client, 
            userId, 
            _messageStore, 
            ChatCompletionsConfig.Combine(_config, config)
        );
    }
    
    public ChatGPT Create(ChatCompletionsConfig? config = null)
    {
        return new ChatGPT(
            _client, 
            _messageStore, 
            ChatCompletionsConfig.Combine(_config, config)
        );
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public class ChatGPTConfig : ChatCompletionsConfig
{
    public string ApiKey { get; set; }
}

