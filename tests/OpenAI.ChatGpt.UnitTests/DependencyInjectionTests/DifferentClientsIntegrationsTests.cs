using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static OpenAI.ChatGpt.AspNetCore.Extensions.ServiceCollectionExtensions;

namespace OpenAI.ChatGpt.UnitTests.DependencyInjectionTests;

public class DifferentClientsIntegrationsTests
{
    [Fact]
    public async void AddAzureOpenAiClient_succeeded()
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
        AssertAiClientOfType<AzureOpenAiClient>(client);
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
                { "AIProvider", "azure_openai"},
                { $"{AzureOpenAiCredentialsConfigSectionPathDefault}:{nameof(AzureOpenAICredentials.ApiKey)}", "test-api-key" },
                { $"{AzureOpenAiCredentialsConfigSectionPathDefault}:{nameof(AzureOpenAICredentials.ApiHost)}", "https://endopoint.openai.azure.com/" },
                { $"{AzureOpenAiCredentialsConfigSectionPathDefault}:{nameof(AzureOpenAICredentials.DeploymentName)}", "deployment" },
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