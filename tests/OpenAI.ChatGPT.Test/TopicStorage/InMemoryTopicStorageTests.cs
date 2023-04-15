using OpenAI.ChatGpt;

namespace OpenAI.ChatGPT.Test.TopicStore;

public class InMemoryTopicStorageTests : AbstractTopicStorageTests
{
    public InMemoryTopicStorageTests() : base(new InMemoryChatHistoryStorage())
    {
    }
}