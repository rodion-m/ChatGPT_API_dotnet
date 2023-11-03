using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static OpenAI.ChatGpt.AspNetCore.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.UnitTests.DependencyInjectionTests;

public class ChatGptServicesIntegrationTests
{
    [Fact]
    public void AddChatGptCoreIntegration_added_expected_services()
    {
        // Arrange
        var services = CreateServiceCollection();

        var initialServiceCount = services.Count;

        // Act
        services.AddChatGptIntegrationCore();

        // Assert
        services.Count.Should().BeGreaterThan(initialServiceCount);
        
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<OpenAICredentials>>();
        provider.GetRequiredService<IOptions<ChatGPTConfig>>();
        
        provider.GetRequiredService<ITimeProvider>();
        provider.GetRequiredService<IOpenAiClient>();
        provider.GetRequiredService<IOpenAiClient>();
    }
    
    [Fact]
    public async void AddChatGptInMemoryIntegration_works()
    {
        // Arrange
        var services = CreateServiceCollection();

        // Act
        services.AddChatGptInMemoryIntegration();

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<InMemoryChatHistoryStorage>();

        var factory = provider.GetRequiredService<ChatGPTFactory>();
        await factory.Create();
        await factory.Create("test-user-id");
    }
    
    [Fact]
    public async void AddChatGptInMemoryIntegration_with_Chat_injection_works()
    {
        // Arrange
        var services = CreateServiceCollection();

        // Act
        services.AddChatGptInMemoryIntegration(injectInMemoryChatService: true);

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<InMemoryChatHistoryStorage>();

        _ = provider.GetRequiredService<ChatService>();
    }
    
    [Fact]
    public async void AddChatGptEntityFrameworkIntegration_works()
    {
        // Arrange
        var services = CreateServiceCollection();

        // Act
        services.AddChatGptEntityFrameworkIntegration(
            options => options.UseInMemoryDatabase("ChatGptInMemoryDb"));

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<EfChatHistoryStorage>();

        var factory = provider.GetRequiredService<ChatGPTFactory>();
        await factory.Create(ensureStorageCreated: true);
        await factory.Create("test-user-id", ensureStorageCreated: true);
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
                    { ChatGPTConfigSectionPathDefault, ""},
                });
            return builder.Build();
        }
    }
}