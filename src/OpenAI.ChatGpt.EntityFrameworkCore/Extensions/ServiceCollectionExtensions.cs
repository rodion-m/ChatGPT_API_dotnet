using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static OpenAI.ChatGpt.AspNetCore.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="IChatHistoryStorage"/> implementation using Entity Framework Core.
    /// </summary>
    public static IServiceCollection AddChatGptEntityFrameworkIntegration(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction,
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = ChatGPTConfigSectionPathDefault,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped,
        bool injectOpenAiClient = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(optionsAction);
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
            credentialsConfigSectionPath, completionsConfigSectionPath, serviceLifetime, injectOpenAiClient);
        services.AddDbContext<ChatGptDbContext>(optionsAction, serviceLifetime);
        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IChatHistoryStorage, EfChatHistoryStorage>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<IChatHistoryStorage, EfChatHistoryStorage>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IChatHistoryStorage, EfChatHistoryStorage>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }
        
        return services;
    }
}