using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OpenAI.ChatGpt.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public const string OpenAiCredentialsConfigSectionPathDefault = "OpenAICredentials";
    public const string AzureOpenAiCredentialsConfigSectionPathDefault = "AzureOpenAICredentials";
    public const string OpenRouterCredentialsConfigSectionPathDefault = "OpenRouterCredentials";

    // ReSharper disable once InconsistentNaming
    public const string ChatGPTConfigSectionPathDefault = "ChatGPTConfig";

    public static IServiceCollection AddChatGptInMemoryIntegration(
        this IServiceCollection services,
        bool injectInMemoryChatService = true,
        string credentialsConfigSectionPath = OpenAiCredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        bool validateAiClientProviderOnStart = true)
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

        services.AddSingleton<IChatHistoryStorage, InMemoryChatHistoryStorage>();
        if (injectInMemoryChatService)
        {
            services.AddScoped<ChatService>(CreateChatService);
        }

        return services.AddChatGptIntegrationCore(
            credentialsConfigSectionPath: credentialsConfigSectionPath,
            completionsConfigSectionPath: completionsConfigSectionPath,
            validateAiClientProviderOnStart: validateAiClientProviderOnStart
        );
    }

    private static ChatService CreateChatService(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var userId = Guid.Empty.ToString();
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        if (storage is not InMemoryChatHistoryStorage)
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

    public static IServiceCollection AddChatGptIntegrationCore(this IServiceCollection services,
        string credentialsConfigSectionPath = OpenAiCredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        string azureOpenAiCredentialsConfigSectionPath = AzureOpenAiCredentialsConfigSectionPathDefault,
        string openRouterCredentialsConfigSectionPath = OpenRouterCredentialsConfigSectionPathDefault,
        ServiceLifetime gptFactoryLifetime = ServiceLifetime.Scoped,
        bool validateAiClientProviderOnStart = true)
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
            .Configure(_ => { }) //make optional
            .ValidateDataAnnotations()
            .ValidateOnStart();  
        services.AddOptions<AzureOpenAICredentials>()
            .BindConfiguration(azureOpenAiCredentialsConfigSectionPath)
            .Configure(_ => { }) //make optional
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<OpenRouterCredentials>()
            .BindConfiguration(openRouterCredentialsConfigSectionPath)
            .Configure(_ => { }) //make optional
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<ChatGPTConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { }) //make optional
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ITimeProvider, TimeProviderUtc>();
        services.Add(new ServiceDescriptor(typeof(ChatGPTFactory), typeof(ChatGPTFactory), gptFactoryLifetime));

        services.AddHttpClient(nameof(OpenAiClient));
        services.AddHttpClient(nameof(AzureOpenAiClient));
        services.AddHttpClient(nameof(OpenRouterClient));

        services.AddSingleton<IAiClient, AiClientFromConfiguration>();
#pragma warning disable CS0618 // Type or member is obsolete
        services.AddSingleton<IOpenAiClient, AiClientFromConfiguration>();
#pragma warning restore CS0618 // Type or member is obsolete

        if (validateAiClientProviderOnStart)
        {
            services.AddHostedService<AiClientStartupValidationBackgroundService>();
        }
        
        return services;
    }
}