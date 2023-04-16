using Microsoft.EntityFrameworkCore;

namespace OpenAI.ChatGpt.UnitTests;

public class EfDbFixture : IDisposable
{
    public EfDbFixture()
    {
        var options = new DbContextOptionsBuilder<ChatGptDbContext>()
            .UseInMemoryDatabase("ChatGptDb")
            .Options;
        ChatGptDbContext = new ChatGptDbContext(options);
        ChatGptDbContext.Database.EnsureCreated();
    }

    public ChatGptDbContext ChatGptDbContext { get; }

    public void Dispose()
    {
        ChatGptDbContext.Dispose();
    }
}