namespace OpenAI.ChatGpt.UnitTests.TopicStorage;

public class EfTopicStorageTests : AbstractTopicStorageTests, IClassFixture<EfDbFixture>
{
    public EfTopicStorageTests(EfDbFixture chatGptDbContext) 
        : base(new EfChatHistoryStorage(chatGptDbContext.ChatGptDbContext))
    {
    }
}