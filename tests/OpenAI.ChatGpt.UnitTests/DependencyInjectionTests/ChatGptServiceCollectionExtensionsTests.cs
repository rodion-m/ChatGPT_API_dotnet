using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static OpenAI.ChatGpt.DependencyInjection.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.UnitTests.DependencyInjectionTests;

public class ChatGptServiceCollectionExtensionsTests
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
        provider.GetRequiredService<IOptions<ChatGptCredentials>>();
        provider.GetRequiredService<IOptions<ChatCompletionsConfig>>();
        
        provider.GetRequiredService<IHttpClientFactory>();
        provider.GetRequiredService<ITimeProvider>();
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
                    { $"{CredentialsConfigSectionPathDefault}:{nameof(ChatGptCredentials.ApiKey)}", "test-api-key" },
                    { CompletionsConfigSectionPathDefault, ""},
                });
            return builder.Build();
        }
    }
}