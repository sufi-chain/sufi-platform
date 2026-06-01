using Microsoft.EntityFrameworkCore;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.Chat.EntityFrameworkCore;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public class ChatDbContext : AbpDbContext<ChatDbContext>, IChatDbContext
{
    public DbSet<ChatSession> Sessions { get; set; }

    public DbSet<ChatMessage> Messages { get; set; }

    public DbSet<ChatParticipant> Participants { get; set; }

    public DbSet<ConversationLink> ConversationLinks { get; set; }

    public DbSet<ChatUsageCounter> UsageCounters { get; set; }

    public DbSet<ChatAiUsageReservation> AiUsageReservations { get; set; }

    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureChat();
    }
}
