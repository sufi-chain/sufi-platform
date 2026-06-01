using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Usage;

public class ChatAiUsageReservation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }
    public virtual Guid SessionId { get; protected set; }
    public virtual Guid? UserId { get; protected set; }
    public virtual Guid? OperatorUserId { get; protected set; }
    public virtual ConversationKind ConversationKind { get; protected set; }
    public virtual AccessMode AccessMode { get; protected set; }
    public virtual ChatAiOperationKind OperationKind { get; protected set; }
    public virtual string? SourceEntityType { get; protected set; }
    public virtual string? SourceEntityId { get; protected set; }
    public virtual string? LinkedEntityType { get; protected set; }
    public virtual string? LinkedEntityId { get; protected set; }
    public virtual int? EstimatedTokens { get; protected set; }
    public virtual int? PromptTokens { get; protected set; }
    public virtual int? CompletionTokens { get; protected set; }
    public virtual int? TotalTokens { get; protected set; }
    public virtual string? ProviderName { get; protected set; }
    public virtual string? WorkspaceName { get; protected set; }
    public virtual string? WalletProviderName { get; protected set; }
    public virtual Guid? WalletId { get; protected set; }
    public virtual string? BillingSubjectType { get; protected set; }
    public virtual string? BillingSubjectId { get; protected set; }
    public virtual bool IsWalletChargeRequired { get; protected set; }
    public virtual string? Currency { get; protected set; }
    public virtual string? DenyReason { get; protected set; }
    public virtual DateTime ReservedAt { get; protected set; }
    public virtual DateTime? RecordedAt { get; protected set; }
    public virtual ChatAiUsageReservationStatus Status { get; protected set; }

    protected ChatAiUsageReservation()
    {
    }

    public ChatAiUsageReservation(
        Guid id,
        ChatAiOperationContext context,
        ChatUsageWalletContext? walletContext,
        DateTime reservedAt)
        : base(id)
    {
        TenantId = context.TenantId;
        SessionId = context.SessionId;
        UserId = context.UserId;
        OperatorUserId = context.OperatorUserId;
        ConversationKind = context.ConversationKind;
        AccessMode = context.AccessMode;
        OperationKind = context.OperationKind;
        EstimatedTokens = context.EstimatedTokens;
        ProviderName = NormalizeOptional(context.ProviderName, ChatConsts.MaxProviderNameLength);
        WorkspaceName = NormalizeOptional(context.WorkspaceName, ChatConsts.MaxWorkspaceNameLength);
        SourceEntityType = NormalizeOptional(context.SourceEntityType, ChatConsts.MaxLinkedEntityTypeLength);
        SourceEntityId = NormalizeOptional(context.SourceEntityId, ChatConsts.MaxLinkedEntityIdLength);
        LinkedEntityType = NormalizeOptional(context.LinkedEntityType, ChatConsts.MaxLinkedEntityTypeLength);
        LinkedEntityId = NormalizeOptional(context.LinkedEntityId, ChatConsts.MaxLinkedEntityIdLength);
        ReservedAt = reservedAt;
        Status = ChatAiUsageReservationStatus.Reserved;
        SetWalletContext(walletContext);
    }

    public virtual void Record(ChatAiUsageRecord record, DateTime recordedAt)
    {
        PromptTokens = record.PromptTokens;
        CompletionTokens = record.CompletionTokens;
        TotalTokens = record.TotalTokens;
        ProviderName = NormalizeOptional(record.ProviderName ?? ProviderName, ChatConsts.MaxProviderNameLength);
        WorkspaceName = NormalizeOptional(record.WorkspaceName ?? WorkspaceName, ChatConsts.MaxWorkspaceNameLength);
        DenyReason = NormalizeOptional(record.DenyReason, ChatConsts.MaxUsageReasonLength);
        RecordedAt = record.RecordedAt ?? recordedAt;
        Status = ChatAiUsageReservationStatus.Recorded;
    }

    public virtual void Release()
    {
        if (Status == ChatAiUsageReservationStatus.Recorded)
        {
            return;
        }

        Status = ChatAiUsageReservationStatus.Released;
    }

    protected virtual void SetWalletContext(ChatUsageWalletContext? walletContext)
    {
        if (walletContext == null)
        {
            return;
        }

        WalletId = walletContext.WalletId;
        WalletProviderName = NormalizeOptional(walletContext.WalletProviderName, ChatConsts.MaxProviderNameLength);
        BillingSubjectType = NormalizeOptional(walletContext.BillingSubjectType, ChatConsts.MaxLinkedEntityTypeLength);
        BillingSubjectId = NormalizeOptional(walletContext.BillingSubjectId, ChatConsts.MaxLinkedEntityIdLength);
        IsWalletChargeRequired = walletContext.IsChargeRequired;
        Currency = NormalizeOptional(walletContext.Currency, ChatConsts.MaxCurrencyLength);
    }

    protected static string? NormalizeOptional(string? value, int maxLength)
    {
        return value.IsNullOrWhiteSpace()
            ? null
            : Check.Length(value, nameof(value), maxLength);
    }
}
