using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        IConfiguration configuration,
        bool injectInMemoryChatService = true,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        string credentialsConfigSectionPath = OpenAiCredentialsConfigSectionPathDefault,
        string azureOpenAiCredentialsConfigSectionPath = AzureOpenAiCredentialsConfigSectionPathDefault,
        string openRouterCredentialsConfigSectionPath = OpenRouterCredentialsConfigSectionPathDefault,
        bool validateAiClientProviderOnStart = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
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

        if (string.IsNullOrWhiteSpace(azureOpenAiCredentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(azureOpenAiCredentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(openRouterCredentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(openRouterCredentialsConfigSectionPath));
        }

        services.AddSingleton<IChatHistoryStorage, InMemoryChatHistoryStorage>();
        if (injectInMemoryChatService)
        {
            services.AddScoped<ChatService>(CreateChatService);
        }

        return services.AddChatGptIntegrationCore(
            configuration,
            completionsConfigSectionPath: completionsConfigSectionPath,
            credentialsConfigSectionPath: credentialsConfigSectionPath,
            azureOpenAiCredentialsConfigSectionPath,
            openRouterCredentialsConfigSectionPath,
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
        IConfiguration configuration,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        string credentialsConfigSectionPath = OpenAiCredentialsConfigSectionPathDefault,
        string azureOpenAiCredentialsConfigSectionPath = AzureOpenAiCredentialsConfigSectionPathDefault,
        string openRouterCredentialsConfigSectionPath = OpenRouterCredentialsConfigSectionPathDefault,
        ServiceLifetime gptFactoryLifetime = ServiceLifetime.Scoped,
        bool validateAiClientProviderOnStart = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
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

        if (string.IsNullOrWhiteSpace(azureOpenAiCredentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(azureOpenAiCredentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(openRouterCredentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(openRouterCredentialsConfigSectionPath));
        }
        
        services.AddOptions<ChatGPTConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { }) //make optional
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ITimeProvider, TimeProviderUtc>();
        services.Add(new ServiceDescriptor(typeof(ChatGPTFactory), typeof(ChatGPTFactory), gptFactoryLifetime));

        services.AddAiClient(configuration, credentialsConfigSectionPath, azureOpenAiCredentialsConfigSectionPath, openRouterCredentialsConfigSectionPath, validateAiClientProviderOnStart);

        return services;
    }

    internal static void AddAiClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string credentialsConfigSectionPath,
        string azureOpenAiCredentialsConfigSectionPath, 
        string openRouterCredentialsConfigSectionPath,
        bool validateAiClientProviderOnStart)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        if (string.IsNullOrWhiteSpace(credentialsConfigSectionPath))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialsConfigSectionPath));
        if (string.IsNullOrWhiteSpace(azureOpenAiCredentialsConfigSectionPath))
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(azureOpenAiCredentialsConfigSectionPath));
        if (string.IsNullOrWhiteSpace(openRouterCredentialsConfigSectionPath))
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(openRouterCredentialsConfigSectionPath));

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

        services.AddHttpClient(nameof(OpenAiClient));
        services.AddHttpClient(nameof(AzureOpenAiClient));
        services.AddHttpClient(nameof(OpenRouterClient));

        services.AddSingleton<IAiClient, AiClientFromConfiguration>();
        services.AddSingleton<AiClientFactory>();
#pragma warning disable CS0618 // Type or member is obsolete
        // will be removed in 5.0
        services.AddSingleton<IOpenAiClient, AiClientFromConfiguration>();
#pragma warning restore CS0618 // Type or member is obsolete

        if (validateAiClientProviderOnStart)
        {
            services.AddHostedService<AiClientStartupValidationBackgroundService>();
        }
    }
}