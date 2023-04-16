using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

#pragma warning disable CS8618

namespace OpenAI.ChatGpt.DependencyInjection.Models;

public class ChatGptCredentials
{
    [Required]
    public string ApiKey { get; set; }

    public string ApiHost { get; set; } = "https://api.openai.com/v1/";

    public AuthenticationHeaderValue GetAuthHeader()
    {
        return new AuthenticationHeaderValue("Bearer", ApiKey);
    }
}