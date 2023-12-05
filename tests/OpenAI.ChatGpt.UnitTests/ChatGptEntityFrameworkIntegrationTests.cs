using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static OpenAI.ChatGpt.AspNetCore.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.UnitTests;

public class ChatGptEntityFrameworkIntegrationTests
{
    [Fact]
    public async void AddChatGptEntityFrameworkIntegration_works()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var services = CreateServiceCollection(configuration);

        // Act
        services.AddChatGptEntityFrameworkIntegration(
            configuration,
            options => options.UseInMemoryDatabase("chats")
        );

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<EfChatHistoryStorage>();

        _ = provider.GetRequiredService<ChatGPTFactory>();
        var client = provider.GetRequiredService<IAiClient>();
        AssertAiClientOfType<OpenAiClient>(client);
        
        var dbContext = provider.GetRequiredService<ChatGptDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    private static ServiceCollection CreateServiceCollection(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);

        return services;
    }
    
    private IConfiguration CreateConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                { $"{OpenAiCredentialsConfigSectionPathDefault}:{nameof(OpenAICredentials.ApiKey)}", "test-api-key" },
                { ChatGPTConfigSectionPathDefault, ""},
            });
            
        return builder.Build();
    }
    
    private void AssertAiClientOfType<T>(IAiClient client)
    {
        if (client is AiClientFromConfiguration aiClientEx)
        {
            aiClientEx.GetInnerClient().Should().BeOfType<T>();
        }
        else
        {
            client.Should().BeOfType<T>();
        }
    }
}