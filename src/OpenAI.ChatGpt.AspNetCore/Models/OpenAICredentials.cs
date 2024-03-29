﻿using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;

#pragma warning disable CS8618

namespace OpenAI.ChatGpt.AspNetCore.Models;

// ReSharper disable once InconsistentNaming
public class OpenAICredentials
{
    private const string DefaultHost = OpenAiClient.DefaultHost;
    
    /// <summary>
    /// OpenAI API key. Can be issued here: https://platform.openai.com/account/api-keys
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
            throw new InvalidOperationException("OpenAI ApiKey is not set");
        }
        ArgumentNullException.ThrowIfNull(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = GetAuthHeader();
        httpClient.BaseAddress = new Uri(ApiHost ?? DefaultHost);
    }
}