using OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

namespace OpenAI.ChatGpt.IntegrationTests.ClientTests;

public class ChatCompletionsVisionTests : IClassFixture<OpenAiClientFixture>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IAiClient _client;

    public ChatCompletionsVisionTests(ITestOutputHelper outputHelper, OpenAiClientFixture fixture)
    {
        _outputHelper = outputHelper;
        _client = fixture.Client;
    }
}