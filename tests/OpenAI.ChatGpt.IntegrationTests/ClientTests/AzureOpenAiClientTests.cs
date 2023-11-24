using OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

namespace OpenAI.ChatGpt.IntegrationTests.ClientTests;

public class AzureOpenAiClientTests : IClassFixture<AzureOpenAiClientFixture>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IOpenAiClient _client;

    public AzureOpenAiClientTests(ITestOutputHelper outputHelper, AzureOpenAiClientFixture fixture)
    {
        _outputHelper = outputHelper;
        _client = fixture.Client;
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