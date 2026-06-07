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
public interface IChatDbContext : IEfCoreDbContext
{
    DbSet<ChatSession> ChatSessions { get; }

    DbSet<ChatMessage> Messages { get; }

    DbSet<ChatParticipant> Participants { get; }

    DbSet<ConversationLink> ConversationLinks { get; }

    DbSet<ChatUsageCounter> UsageCounters { get; }

    DbSet<ChatAiUsageReservation> AiUsageReservations { get; }
}
