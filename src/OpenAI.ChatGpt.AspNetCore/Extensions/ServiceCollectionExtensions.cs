using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.AspNetCore.Models;

namespace OpenAI.ChatGpt.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CredentialsConfigSectionPathDefault = "OpenAICredentials";
    // ReSharper disable once InconsistentNaming
    public const string ChatGPTConfigSectionPathDefault = "ChatGPTConfig";
    
    public static IServiceCollection AddChatGptInMemoryIntegration(
        this IServiceCollection services,
        bool injectInMemoryChatService = true,
        bool injectOpenAiClient = true,
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(credentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(credentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(completionsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(completionsConfigSectionPath));
        }
        services.AddChatGptIntegrationCore(
            credentialsConfigSectionPath,
            completionsConfigSectionPath, 
            injectOpenAiClient: injectOpenAiClient
        );
        services.AddSingleton<IChatHistoryStorage, InMemoryChatHistoryStorage>();
        if(injectInMemoryChatService)
        {
            services.AddScoped<ChatService>(CreateChatService);
        }
        
        return services;
    }

    private static ChatService CreateChatService(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var userId = Guid.Empty.ToString();
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        if(storage is not InMemoryChatHistoryStorage)
        {
            throw new InvalidOperationException(
                $"Chat injection is supported only with {nameof(InMemoryChatHistoryStorage)} " +
                $"and is not supported for {storage.GetType().FullName}");
        }
        /*
         * .GetAwaiter().GetResult() are safe here because we use sync in memory storage
         */
        var chatGpt = provider.GetRequiredService<ChatGPTFactory>()
            .Create(userId)
            .GetAwaiter()
            .GetResult();
        var chat = chatGpt.StartNewTopic(clearOnDisposal: true).GetAwaiter().GetResult();
        return chat;
    }

    public static IServiceCollection AddChatGptIntegrationCore(
        this IServiceCollection services, 
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
        bool injectOpenAiClient = true
        )
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(credentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(credentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(completionsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(completionsConfigSectionPath));
        }

        services.AddOptions<OpenAICredentials>()
            .BindConfiguration(credentialsConfigSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<ChatGPTConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { }) //optional
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (services.All(it => it.ServiceType != typeof(IHttpClientFactory)))
        {
            services.AddHttpClient(OpenAiClient.HttpClientName);
        }

        services.AddSingleton<ITimeProvider, TimeProviderUtc>();
        services.Add(new ServiceDescriptor(typeof(ChatGPTFactory), typeof(ChatGPTFactory), serviceLifetime));

        if (injectOpenAiClient)
        {
            AddOpenAiClient(services);
        }
        
        return services;
    }

    private static void AddOpenAiClient(IServiceCollection services)
    {
        services.AddSingleton<IOpenAiClient>(provider =>
        {
            var credentials = provider.GetRequiredService<IOptions<OpenAICredentials>>().Value;
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(OpenAiClient.HttpClientName);
            httpClient.DefaultRequestHeaders.Authorization = credentials.GetAuthHeader();
            httpClient.BaseAddress = new Uri(credentials.ApiHost);
            var client = new OpenAiClient(httpClient);
            return client;
        });
    }
}