using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Participants;

public class ChatParticipant : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid SessionId { get; protected set; }

    public virtual Guid? UserId { get; protected set; }

    public virtual string? AnonymousVisitorId { get; protected set; }

    public virtual ChatMessageSenderKind ParticipantKind { get; protected set; }

    public virtual string? DisplayName { get; protected set; }

    public virtual DateTime JoinedAt { get; protected set; }

    public virtual DateTime? LeftAt { get; protected set; }

    protected ChatParticipant()
    {
    }

    public ChatParticipant(
        Guid id,
        Guid? tenantId,
        Guid sessionId,
        ChatMessageSenderKind participantKind,
        DateTime joinedAt,
        Guid? userId = null,
        string? anonymousVisitorId = null,
        string? displayName = null)
        : base(id)
    {
        TenantId = tenantId;
        SessionId = sessionId;
        ParticipantKind = participantKind;
        JoinedAt = joinedAt;
        UserId = userId;
        AnonymousVisitorId = anonymousVisitorId.IsNullOrWhiteSpace()
            ? null
            : Check.Length(anonymousVisitorId, nameof(anonymousVisitorId), ChatConsts.MaxAnonymousVisitorIdLength);
        SetDisplayName(displayName);
        EnsureValidIdentity();
    }

    public virtual void SetDisplayName(string? displayName)
    {
        DisplayName = displayName.IsNullOrWhiteSpace()
            ? null
            : Check.Length(displayName, nameof(displayName), ChatConsts.MaxDisplayNameLength);
    }

    public virtual void Leave(DateTime leftAt)
    {
        LeftAt = leftAt;
    }

    protected virtual void EnsureValidIdentity()
    {
        if (UserId.HasValue == !AnonymousVisitorId.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ChatErrorCodes.InvalidParticipant);
        }
    }
}
