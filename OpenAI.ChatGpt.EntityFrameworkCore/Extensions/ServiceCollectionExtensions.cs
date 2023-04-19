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
        string completionsConfigSectionPath = CchatGPTConfigSectionPathDefault)
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
        
        services.AddChatGptIntegrationCore(credentialsConfigSectionPath, completionsConfigSectionPath);
        services.AddDbContext<ChatGptDbContext>(optionsAction);
        services.AddScoped<IChatHistoryStorage, EfChatHistoryStorage>();
        return services;
    }
}