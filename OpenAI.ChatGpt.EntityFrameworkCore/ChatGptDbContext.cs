using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class ChatGptDbContext : DbContext
{
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<PersistentChatMessage> Messages => Set<PersistentChatMessage>();

    public ChatGptDbContext(DbContextOptions<ChatGptDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Topic>().OwnsOne(it => it.Config);
    }
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            configurationBuilder
                .Properties<DateTimeOffset>()
                .HaveConversion<DateTimeOffsetToBinaryConverter>();

            configurationBuilder
                .Properties<DateTimeOffset?>()
                .HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }
}