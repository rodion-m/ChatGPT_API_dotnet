using OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

namespace OpenAI.ChatGpt.IntegrationTests.ClientTests;

public class OpenRouterClientTests
{
    public class ChatCompletionsApiTests : IClassFixture<OpenRouterClientFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IAiClient _client;

        public ChatCompletionsApiTests(ITestOutputHelper outputHelper, OpenRouterClientFixture fixture)
        {
            _outputHelper = outputHelper;
            _client = fixture.Client;
        }

        [Fact]
        public async void Get_response_from_mistral7B_for_one_message()
        {
            string model = "mistralai/mistral-7b-instruct";
            var dialog = Dialog.StartAsUser("Who are you?");
            string response = await _client.GetChatCompletions(dialog, 80, model: model);
            _outputHelper.WriteLine(response);
            response.Should().NotBeNullOrEmpty();
        }
    }
}