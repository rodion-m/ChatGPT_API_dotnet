namespace OpenAI.ChatGpt;

/// <summary>
///  Azure OpenAI services client
/// </summary>
/// <remarks>
/// Docs: https://learn.microsoft.com/en-us/azure/ai-services/openai/reference
/// Models availability by zones: https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models
/// </remarks>
public class AzureOpenAiClient : OpenAiClient
{
    private readonly string _apiVersion;
    private const string DefaultApiVersion = "2023-12-01-preview";

    //https://github.com/Azure/azure-rest-api-specs/tree/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference
    
    /// <summary>
    /// Creates Azure OpenAI services client
    /// </summary>
    /// <param name="endpointUrl"> Endpoint URL like https://{your-resource-name}.openai.azure.com/</param>
    /// <param name="deploymentName"> Deployment name from the page https://oai.azure.com/deployment</param>
    /// <param name="azureKey">Azure OpenAI API Key</param>
    /// <param name="apiVersion">Azure OpenAI API version</param>
    /// <remarks>
    /// See currently available API versions: https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#completions
    /// </remarks>
    public AzureOpenAiClient(
        string endpointUrl, 
        string deploymentName, 
        string azureKey, 
        string apiVersion = DefaultApiVersion)
    {
        if (string.IsNullOrWhiteSpace(azureKey))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(azureKey));
        
        _apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri($"{endpointUrl}/openai/deployments/{deploymentName}/"),
            DefaultRequestHeaders =
            {
                { "api-key", azureKey }
            }
        };
        IsHttpClientInjected = false;
    }

    public AzureOpenAiClient(HttpClient httpClient, string apiVersion) : base(httpClient)
    {
        _apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
    }

    protected override string GetChatCompletionsEndpoint()
    {
        return $"chat/completions?api-version={_apiVersion}";
    }
}