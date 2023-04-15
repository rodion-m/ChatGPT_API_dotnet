using OpenAI.ChatGpt.EntityFrameworkCore;

namespace OpenAI.ChatGPT.Test.MessageStorage;

public class EfMessageStorageTests : AbstractMessageStorageTests, IClassFixture<EfDbFixture>
{
    public EfMessageStorageTests(EfDbFixture chatGptDbContext) 
        : base(new EfChatHistoryStorage(chatGptDbContext.ChatGptDbContext))
    {
    }
}