using Microsoft.EntityFrameworkCore;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt.EntityFrameworkCore;

public class ChatGptDbContext : DbContext
{
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<PersistentChatMessage> Messages => Set<PersistentChatMessage>();

    public ChatGptDbContext(DbContextOptions<ChatGptDbContext> options) : base(options)
    {
    }
}