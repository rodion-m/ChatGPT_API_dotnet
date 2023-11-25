using Microsoft.Extensions.Options;

namespace OpenAI.ChatGpt.AspNetCore;

internal class AiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAICredentials _openAiCredentials;
    private readonly AzureOpenAICredentials _azureOpenAiCredentials;
    private readonly OpenRouterCredentials _openRouterCredentials;

    public AiClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenAICredentials> openAiCredentialsOptions,
        IOptions<AzureOpenAICredentials> azureOpenAiCredentialsOptions,
        IOptions<OpenRouterCredentials> openRouterCredentialsOptions)
    {
        ArgumentNullException.ThrowIfNull(openAiCredentialsOptions);
        ArgumentNullException.ThrowIfNull(azureOpenAiCredentialsOptions);
        ArgumentNullException.ThrowIfNull(openRouterCredentialsOptions);
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _openAiCredentials = openAiCredentialsOptions.Value;
        _azureOpenAiCredentials = azureOpenAiCredentialsOptions.Value;
        _openRouterCredentials = openRouterCredentialsOptions.Value;
    }

    public OpenAiClient GetOpenAiClient()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(OpenAiClient));
        if (_openAiCredentials.ApiKey is null)
        {
            throw new InvalidOperationException(
                $"OpenAI API key is not configured. Please configure it in {nameof(OpenAICredentials)}");
        }
        _openAiCredentials.SetupHttpClient(httpClient);
        return new OpenAiClient(httpClient);
    }

    public AzureOpenAiClient GetAzureOpenAiClient()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(AzureOpenAiClient));
        if (_azureOpenAiCredentials.ApiKey is null)
        {
            throw new InvalidOperationException(
                $"Azure OpenAI API key is not configured. Please configure it in {nameof(AzureOpenAICredentials)}");
        }
        _azureOpenAiCredentials.SetupHttpClient(httpClient);
        return new AzureOpenAiClient(httpClient);
    }

    public OpenRouterClient GetOpenRouterClient()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(OpenRouterClient));
        if (_openRouterCredentials.ApiKey is null)
        {
            throw new InvalidOperationException(
                $"OpenRouter API key is not configured. Please configure it in {nameof(OpenRouterCredentials)}");
        }
        _openRouterCredentials.SetupHttpClient(httpClient);
        return new OpenRouterClient(httpClient);
    }
}