using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.Chat.EntityFrameworkCore;

public static class ChatDbContextModelCreatingExtensions
{
    public static void ConfigureChat(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ChatSession>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "Sessions", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureFullAuditedAggregateRoot();
            b.ConfigureMultiTenant();

            b.Property(x => x.Title).HasMaxLength(ChatConsts.MaxTitleLength);
            b.Property(x => x.AccessMode).IsRequired();
            b.Property(x => x.ConversationKind).IsRequired();
            b.Property(x => x.ChannelOrigin).IsRequired();
            b.Property(x => x.Status).IsRequired();
            b.Property(x => x.MetadataJson).HasMaxLength(ChatConsts.MaxMetadataJsonLength);

            b.HasIndex(x => new { x.TenantId, x.Status, x.LastMessageTime });
            b.HasIndex(x => new { x.TenantId, x.AccessMode, x.LastMessageTime });
            b.HasIndex(x => x.LastMessageTime);
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "Messages", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureMultiTenant();

            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.Body).IsRequired().HasMaxLength(ChatConsts.MaxMessageBodyLength);
            b.Property(x => x.SenderKind).IsRequired();
            b.Property(x => x.AnonymousVisitorId).HasMaxLength(ChatConsts.MaxAnonymousVisitorIdLength);
            b.Property(x => x.IsInternal).IsRequired();
            b.Property(x => x.MetadataJson).HasMaxLength(ChatConsts.MaxMetadataJsonLength);
            b.Property(x => x.AttachmentFileIds)
                .HasConversion(
                    value => string.Join(',', value),
                    value => value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<Guid>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    value => value.ToList()));

            b.Property(x => x.AttachmentFileIds)
                .HasMaxLength(4000);

            b.HasIndex(x => new { x.SessionId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.SessionId, x.CreationTime });
        });

        builder.Entity<ChatParticipant>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "Participants", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureMultiTenant();

            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.AnonymousVisitorId).HasMaxLength(ChatConsts.MaxAnonymousVisitorIdLength);
            b.Property(x => x.ParticipantKind).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(ChatConsts.MaxDisplayNameLength);
            b.Property(x => x.JoinedAt).IsRequired();

            b.HasIndex(x => new { x.TenantId, x.SessionId, x.UserId });
            b.HasIndex(x => new { x.TenantId, x.SessionId, x.AnonymousVisitorId });
            b.HasIndex(x => new { x.TenantId, x.UserId, x.LeftAt });
        });

        builder.Entity<ConversationLink>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "ConversationLinks", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureMultiTenant();

            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.LinkedEntityType).IsRequired().HasMaxLength(ChatConsts.MaxLinkedEntityTypeLength);
            b.Property(x => x.LinkedEntityId).IsRequired().HasMaxLength(ChatConsts.MaxLinkedEntityIdLength);
            b.Property(x => x.LinkRole).HasMaxLength(ChatConsts.MaxLinkRoleLength);
            b.Property(x => x.MetadataJson).HasMaxLength(ChatConsts.MaxMetadataJsonLength);

            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => new { x.LinkedEntityType, x.LinkedEntityId });
            b.HasIndex(x => new { x.TenantId, x.LinkedEntityType, x.LinkedEntityId });
        });

        builder.Entity<ChatUsageCounter>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "UsageDailyAggregates", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureMultiTenant();

            b.Property(x => x.CounterKey).IsRequired().HasMaxLength(ChatConsts.MaxUsageCounterKeyLength);
            b.Property(x => x.Period).IsRequired();
            b.Property(x => x.PeriodStart).IsRequired();
            b.Property(x => x.PeriodEnd).IsRequired();
            b.Property(x => x.Count).IsRequired();
            b.Property(x => x.TokenCount).IsRequired();

            b.HasIndex(x => new { x.TenantId, x.CounterKey, x.Period, x.PeriodStart }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.PeriodStart });
        });

        builder.Entity<ChatAiUsageReservation>(b =>
        {
            b.ToTable(ChatDbProperties.DbTablePrefix + "AiUsageReservations", ChatDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.ConfigureFullAuditedAggregateRoot();
            b.ConfigureMultiTenant();

            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.ConversationKind).IsRequired();
            b.Property(x => x.AccessMode).IsRequired();
            b.Property(x => x.OperationKind).IsRequired();
            b.Property(x => x.SourceEntityType).HasMaxLength(ChatConsts.MaxLinkedEntityTypeLength);
            b.Property(x => x.SourceEntityId).HasMaxLength(ChatConsts.MaxLinkedEntityIdLength);
            b.Property(x => x.LinkedEntityType).HasMaxLength(ChatConsts.MaxLinkedEntityTypeLength);
            b.Property(x => x.LinkedEntityId).HasMaxLength(ChatConsts.MaxLinkedEntityIdLength);
            b.Property(x => x.ProviderName).HasMaxLength(ChatConsts.MaxProviderNameLength);
            b.Property(x => x.WorkspaceName).HasMaxLength(ChatConsts.MaxWorkspaceNameLength);
            b.Property(x => x.WalletProviderName).HasMaxLength(ChatConsts.MaxProviderNameLength);
            b.Property(x => x.BillingSubjectType).HasMaxLength(ChatConsts.MaxLinkedEntityTypeLength);
            b.Property(x => x.BillingSubjectId).HasMaxLength(ChatConsts.MaxLinkedEntityIdLength);
            b.Property(x => x.Currency).HasMaxLength(ChatConsts.MaxCurrencyLength);
            b.Property(x => x.DenyReason).HasMaxLength(ChatConsts.MaxUsageReasonLength);
            b.Property(x => x.ReservedAt).IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.HasIndex(x => new { x.TenantId, x.ReservedAt });
            b.HasIndex(x => new { x.TenantId, x.SessionId, x.OperationKind, x.ReservedAt });
            b.HasIndex(x => new { x.SessionId, x.OperationKind, x.Status });
            b.HasIndex(x => new { x.OperatorUserId, x.OperationKind, x.ReservedAt });
            b.HasIndex(x => new { x.TenantId, x.OperationKind, x.RecordedAt });
        });
    }
}
