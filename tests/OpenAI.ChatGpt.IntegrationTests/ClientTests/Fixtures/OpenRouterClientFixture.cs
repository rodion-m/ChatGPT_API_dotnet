namespace OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

public class OpenRouterClientFixture
{
    public IAiClient Client { get; private set; }
        = new OpenRouterClient(Helpers.GetOpenRouterKey());
}