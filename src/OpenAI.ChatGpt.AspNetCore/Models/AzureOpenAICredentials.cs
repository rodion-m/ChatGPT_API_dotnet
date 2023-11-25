using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace OpenAI.ChatGpt.AspNetCore.Models;

public class AzureOpenAICredentials
{
    /// <summary>
    /// Azure OpenAI API key from Azure Portal.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Azure Open AI API endpoint url.
    /// </summary>
    [Url]
    public string? ApiHost { get; set; }

    public string? DeploymentName { get; set; }
    
    public AuthenticationHeaderValue GetAuthHeader()
    {
        return new AuthenticationHeaderValue("Bearer", ApiKey);
    }

    public void SetupHttpClient(HttpClient httpClient)
    {
        if (ApiKey is null)
        {
            throw new InvalidOperationException("ApiKey is null");
        }
        if (ApiHost is null)
        {
            throw new InvalidOperationException("ApiHost is null");
        }
        if (DeploymentName is null)
        {
            throw new InvalidOperationException("DeploymentName is null");
        }
        AzureOpenAiClient.SetupHttpClient(httpClient, ApiHost, DeploymentName, ApiKey ?? throw new InvalidOperationException("ApiKey is null"));
    }
}