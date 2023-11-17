namespace OpenAI.ChatGpt.IntegrationTests.OpenAiClientTests;

public class AzureOpenAiClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IOpenAiClient _client;

    public AzureOpenAiClientTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        var endpointUrl = Helpers.GetValueFromConfiguration("AZURE_OPENAI_ENDPOINT_URL");
        var azureKey = Helpers.GetValueFromConfiguration("AZURE_OPENAI_API_KEY");
        var deploymentName = Helpers.GetValueFromConfiguration("AZURE_OPENAI_DEPLOYMENT_NAME");
        _client = new AzureOpenAiClient(endpointUrl, deploymentName, azureKey, "2023-12-01-preview");
    }

    [Fact]
    public async void Get_chatgpt_response_for_one_message_works()
    {
        string text = "Who are you? In two words.";
        string response = await _client.GetChatCompletions(new UserMessage(text), 64);
        _outputHelper.WriteLine(response);
        response.Should().NotBeNullOrEmpty();
    }
}