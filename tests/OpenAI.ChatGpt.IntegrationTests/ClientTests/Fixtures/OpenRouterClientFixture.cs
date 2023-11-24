namespace OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

public class OpenRouterClientFixture
{
    public IOpenAiClient Client { get; private set; }
        = new OpenRouterClient(Helpers.GetOpenRouterKey());
}