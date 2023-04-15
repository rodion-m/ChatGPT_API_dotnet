using OpenAI.ChatGpt;

namespace OpenAI.ChatGPT.Test.MessageStorage;

public class InMemoryMessageStorageTests : AbstractMessageStorageTests
{
    public InMemoryMessageStorageTests() : base(new InMemoryChatHistoryStorage())
    {
    }
}