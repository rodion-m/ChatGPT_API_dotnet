namespace OpenAI.ChatGpt.IntegrationTests.ClientTests.Fixtures;

public class OpenAiClientFixture
{
    public IOpenAiClient Client { get; private set; } 
        = new OpenAiClient(Helpers.GetOpenAiKey());
}