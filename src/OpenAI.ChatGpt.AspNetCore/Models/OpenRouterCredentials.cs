using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

namespace OpenAI.ChatGpt.AspNetCore.Models;

public class OpenRouterCredentials
{
    private const string DefaultHost = OpenRouterClient.DefaultHost;
    
    /// <summary>
    /// OpenRouter API key. Can be issued here: https://openrouter.ai/keys
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Open AI API host. Default is: <see cref="DefaultHost"/>
    /// </summary>
    [Url]
    public string? ApiHost { get; set; } = DefaultHost;

    public AuthenticationHeaderValue GetAuthHeader()
    {
        return new AuthenticationHeaderValue("Bearer", ApiKey);
    }

    public void SetupHttpClient(HttpClient httpClient)
    {
        if (ApiKey is null)
        {
            throw new InvalidOperationException("OpenRouter ApiKey is not set");
        }
        ArgumentNullException.ThrowIfNull(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader();
        httpClient.BaseAddress = new Uri(ApiHost ?? DefaultHost);
    }
}