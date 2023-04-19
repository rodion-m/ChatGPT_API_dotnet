using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static OpenAI.ChatGpt.AspNetCore.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.IntegrationTests;

public class ChatGptEntityFrameworkIntegrationTests
{
    [Fact]
    public async void AddChatGptEntityFrameworkIntegration_works()
    {
        // Arrange
        var services = CreateServiceCollection();

        // Act
        services.AddChatGptEntityFrameworkIntegration(
            options => options.UseSqlite(connectionString: "Data Source=chatgpt.db"));

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<EfChatHistoryStorage>();

        _ = provider.GetRequiredService<ChatGPTFactory>();
        
        var dbContext = provider.GetRequiredService<ChatGptDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    private static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(CreateConfiguration());
        return services;

        IConfiguration CreateConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>()
                {
                    { $"{CredentialsConfigSectionPathDefault}:{nameof(OpenAICredentials.ApiKey)}", "test-api-key" },
                    { CchatGPTConfigSectionPathDefault, ""},
                });
            return builder.Build();
        }
    }
}