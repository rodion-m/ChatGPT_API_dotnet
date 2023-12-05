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
        var configuration = CreateConfiguration();
        var services = CreateServiceCollection(configuration);

        var initialServiceCount = services.Count;

        // Act
        services.AddChatGptIntegrationCore(configuration);

        // Assert
        services.Count.Should().BeGreaterThan(initialServiceCount);
        
        using var provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<IOptions<OpenAICredentials>>();
        _ = provider.GetRequiredService<IOptions<ChatGPTConfig>>();
        
        _ = provider.GetRequiredService<ITimeProvider>();
        _ = provider.GetRequiredService<IAiClient>();
    }
    
    [Fact]
    public async void AddChatGptInMemoryIntegration_works()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var services = CreateServiceCollection(configuration);

        // Act
        services.AddChatGptInMemoryIntegration(configuration);

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
        var configuration = CreateConfiguration();
        var services = CreateServiceCollection(configuration);

        // Act
        services.AddChatGptInMemoryIntegration(configuration, injectInMemoryChatService: true);

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
        var configuration = CreateConfiguration();
        var services = CreateServiceCollection(configuration);

        // Act
        services.AddChatGptEntityFrameworkIntegration(
            configuration,
            options => options.UseInMemoryDatabase("ChatGptInMemoryDb")
        );

        // Assert
        await using var provider = services.BuildServiceProvider();
        
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        storage.Should().BeOfType<EfChatHistoryStorage>();

        var factory = provider.GetRequiredService<ChatGPTFactory>();
        await factory.Create(ensureStorageCreated: true);
        await factory.Create("test-user-id", ensureStorageCreated: true);
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
}