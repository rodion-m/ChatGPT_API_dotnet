using Microsoft.Extensions.DependencyInjection;
using OpenAI.ChatGpt.AspNetCore.Models;

namespace OpenAI.ChatGpt.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CredentialsConfigSectionPathDefault = "ChatGptCredentials";
    public const string CompletionsConfigSectionPathDefault = "ChatCompletionsConfig";
    
    public static IServiceCollection AddChatGptInMemoryIntegration(
        this IServiceCollection services,
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = CompletionsConfigSectionPathDefault)
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
        services.AddChatGptIntegrationCore(credentialsConfigSectionPath, completionsConfigSectionPath);
        services.AddSingleton<IChatHistoryStorage, InMemoryChatHistoryStorage>();
        return services;
    }
    public static IServiceCollection AddChatGptIntegrationCore(
        this IServiceCollection services, 
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = CompletionsConfigSectionPathDefault)
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

        services.AddOptions<ChatGptCredentials>()
            .BindConfiguration(credentialsConfigSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<ChatCompletionsConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { }) //optional
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddHttpClient();

        services.AddSingleton<ITimeProvider, TimeProviderUtc>();
        services.AddSingleton<ChatGPTFactory>();

        return services;
    }
}