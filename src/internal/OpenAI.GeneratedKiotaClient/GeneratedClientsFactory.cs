using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenAI.Azure.GeneratedKiotaClient;

namespace OpenAI.GeneratedKiotaClient;

internal static class GeneratedClientsFactory
{
    public static GeneratedOpenAiClient CreateGeneratedOpenAiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
        var openAiClient = new GeneratedOpenAiClient(adapter);
        return openAiClient;
    }
    
    public static GeneratedAzureOpenAiClient CreateGeneratedAzureOpenAiClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        var authProvider = new AnonymousAuthenticationProvider();
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
        return new GeneratedAzureOpenAiClient(adapter);
    }
}