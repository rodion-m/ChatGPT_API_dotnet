namespace OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

public class OpenAiClientFixture
{
    public IAiClient Client { get; private set; } 
        = new OpenAiClient(Helpers.GetOpenAiKey());
}