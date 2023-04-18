using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

#pragma warning disable CS8618

namespace OpenAI.ChatGpt.AspNetCore.Models;

public class ChatGptCredentials
{
    /// <summary>
    /// OpenAI API key. Can be issued here: https://platform.openai.com/account/api-keys
    /// </summary>
    [Required]
    public string ApiKey { get; set; }

    [Url]
    public string ApiHost { get; set; } = "https://api.openai.com/v1/";

    public AuthenticationHeaderValue GetAuthHeader()
    {
        return new AuthenticationHeaderValue("Bearer", ApiKey);
    }
}