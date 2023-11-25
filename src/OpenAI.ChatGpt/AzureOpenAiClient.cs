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

    /// <summary>
    /// Creates Azure OpenAI services client
    /// </summary>
    /// <param name="endpointUrl"> Endpoint URL like https://{your-resource-name}.openai.azure.com/</param>
    /// <param name="deploymentName"> Deployment name from the page https://oai.azure.com/deployment</param>
    /// <param name="azureKey">Azure OpenAI API Key</param>
    /// <param name="apiVersion">Azure OpenAI API version</param>
    /// <remarks>
    /// See currently available API versions: https://learn.microsoft.com/en-us/azure/ai-services/openai/reference#completions
    /// Specifications: https://github.com/Azure/azure-rest-api-specs/tree/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference
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
        HttpClient = new HttpClient();
        SetupHttpClient(HttpClient, endpointUrl, deploymentName, azureKey);
        IsHttpClientInjected = false;
    }

    internal static void SetupHttpClient(HttpClient httpClient, string endpointUrl, string deploymentName, string azureKey)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(endpointUrl);
        ArgumentNullException.ThrowIfNull(deploymentName);
        ArgumentNullException.ThrowIfNull(azureKey);
        httpClient.BaseAddress = new Uri($"{endpointUrl}/openai/deployments/{deploymentName}/");
        httpClient.DefaultRequestHeaders.Add("api-key", azureKey);
    }

    public AzureOpenAiClient(HttpClient httpClient, string apiVersion) : base(httpClient)
    {
        _apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
    }
    
    public AzureOpenAiClient(HttpClient httpClient) : base(httpClient)
    {
        _apiVersion = DefaultApiVersion;
    }

    protected override string GetChatCompletionsEndpoint()
    {
        return $"chat/completions?api-version={_apiVersion}";
    }
}