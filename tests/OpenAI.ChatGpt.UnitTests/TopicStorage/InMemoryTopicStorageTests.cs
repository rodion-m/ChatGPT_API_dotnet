namespace OpenAI.ChatGpt.UnitTests.TopicStorage;

public class InMemoryTopicStorageTests : AbstractTopicStorageTests
{
    public InMemoryTopicStorageTests() : base(new InMemoryChatHistoryStorage())
    {
    }
}