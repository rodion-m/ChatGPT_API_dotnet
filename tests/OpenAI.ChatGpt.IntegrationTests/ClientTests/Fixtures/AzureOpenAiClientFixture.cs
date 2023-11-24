namespace OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

public class AzureOpenAiClientFixture
{
    public IOpenAiClient Client { get; }

    public AzureOpenAiClientFixture()
    {
        var endpointUrl = Helpers.GetRequiredValueFromConfiguration("AZURE_OPENAI_ENDPOINT_URL");
        var azureKey = Helpers.GetRequiredValueFromConfiguration("AZURE_OPENAI_API_KEY");
        var deploymentName = Helpers.GetRequiredValueFromConfiguration("AZURE_OPENAI_DEPLOYMENT_NAME");
        Client = new AzureOpenAiClient(endpointUrl, deploymentName, azureKey, "2023-12-01-preview");
    }
}