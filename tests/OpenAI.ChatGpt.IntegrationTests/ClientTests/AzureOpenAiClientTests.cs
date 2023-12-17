using OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

namespace OpenAI.ChatGpt.IntegrationTests.ClientTests;

public class AzureOpenAiClientTests : IClassFixture<AzureOpenAiClientFixture>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly IAiClient _client;

    public AzureOpenAiClientTests(ITestOutputHelper outputHelper, AzureOpenAiClientFixture fixture)
    {
        _outputHelper = outputHelper;
        _client = fixture.Client;
    }

    [Fact]
    public async void Get_response_from_GPT4_32k_model_for_one_message_works()
    {
        string text = "Who are you? In two words.";
#pragma warning disable CS0618 // Type or member is obsolete
        string response = await _client.GetChatCompletions(new UserMessage(text), model: ChatCompletionModels.Gpt4_32k);
#pragma warning restore CS0618 // Type or member is obsolete
        _outputHelper.WriteLine(response);
        response.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async void Get_long_response_from_gpt4_Turbo_model()
    {
        string text = "Describe who are you in a very detailed way. At least 300 words.";
        string response = await _client.GetChatCompletions(new UserMessage(text), model: ChatCompletionModels.Gpt4Turbo);
        _outputHelper.WriteLine(response);
        response.Should().NotBeNullOrEmpty();
    }
}