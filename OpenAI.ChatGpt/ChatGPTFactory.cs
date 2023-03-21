﻿using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt;

/// <summary>
///  Factory for creating <see cref="ChatGPT" /> instances from DI.
/// </summary>
/// <example>
/// builder.Services.AddHttpClient&lt;ChatGPTFactory&lt;(client =>
///     {
///         client.DefaultRequestHeaders.Authorization = builder.Configuration["ChatGPTCredentials:ApiKey"];
///     })
///     .AddPolicyHandler(GetRetryPolicy())
///     .AddPolicyHandler(GetCircuitBreakerPolicy());
/// </example>
// ReSharper disable once InconsistentNaming
internal class ChatGPTFactory : IDisposable
{
    private readonly OpenAiClient _client;
    private readonly ChatCompletionsConfig _config;
    private readonly IMessageStore _messageStore;

    public ChatGPTFactory(
        HttpClient httpClient,
        IOptions<ChatCompletionsConfig> config,
        IMessageStore messageStore)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _client = new OpenAiClient(httpClient);
    }

    public ChatGPTFactory(
        IOptions<ChatGPTCredentials> credentials,
        IOptions<ChatCompletionsConfig> config,
        IMessageStore messageStore)
    {
        if (credentials?.Value == null) throw new ArgumentNullException(nameof(credentials));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _client = new OpenAiClient(credentials.Value.ApiKey);
    }

    public ChatGPTFactory(
        string apiKey,
        IMessageStore messageStore,
        ChatCompletionsConfig? config = null)
    {
        if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
        _client = new OpenAiClient(apiKey);
        _config = config ?? ChatCompletionsConfig.Default;
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

internal class ChatGPTCredentials
{
    public string ApiKey { get; set; }
    public string? ApiHost { get; set; }
    
    public AuthenticationHeaderValue AuthHeader() 
        => new("Bearer", ApiKey);
}