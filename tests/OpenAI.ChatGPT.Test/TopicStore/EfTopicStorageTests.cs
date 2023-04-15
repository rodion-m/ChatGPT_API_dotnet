using OpenAI.ChatGpt.EntityFrameworkCore;

namespace OpenAI.ChatGPT.Test.TopicStore;

public class EfTopicStorageTests : AbstractTopicStorageTests, IClassFixture<EfDbFixture>
{
    public EfTopicStorageTests(EfDbFixture chatGptDbContext) 
        : base(new EfChatHistoryStorage(chatGptDbContext.ChatGptDbContext))
    {
    }
}