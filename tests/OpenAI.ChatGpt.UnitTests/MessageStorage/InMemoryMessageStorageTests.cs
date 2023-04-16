namespace OpenAI.ChatGpt.UnitTests.MessageStorage;

public class InMemoryMessageStorageTests : AbstractMessageStorageTests
{
    public InMemoryMessageStorageTests() : base(new InMemoryChatHistoryStorage())
    {
    }
}