namespace OpenAI.ChatGpt.UnitTests.MessageStorage;

public class EfMessageStorageTests : AbstractMessageStorageTests, IClassFixture<EfDbFixture>
{
    public EfMessageStorageTests(EfDbFixture chatGptDbContext) 
        : base(new EfChatHistoryStorage(chatGptDbContext.ChatGptDbContext))
    {
    }
}