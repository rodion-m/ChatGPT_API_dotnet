using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

#pragma warning disable CS8618

namespace OpenAI.ChatGpt.AspNetCore.Models;

// ReSharper disable once InconsistentNaming
public class OpenAICredentials
{
    private const string DefaultHost = "https://api.openai.com/v1/";
    
    /// <summary>
    /// OpenAI API key. Can be issued here: https://platform.openai.com/account/api-keys
    /// </summary>
    [Required]
    public string ApiKey { get; set; }

    /// <summary>
    /// Open AI API host. Default is: <see cref="DefaultHost"/>
    /// </summary>
    [Url]
    public string ApiHost { get; set; } = DefaultHost;

    public AuthenticationHeaderValue GetAuthHeader()
    {
        return new AuthenticationHeaderValue("Bearer", ApiKey);
    }
}