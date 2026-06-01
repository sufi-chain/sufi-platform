using System;
using SufiChain.Chat.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Sessions;

public class ChatSession : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string? Title { get; protected set; }

    public virtual AccessMode AccessMode { get; protected set; }

    public virtual ConversationKind ConversationKind { get; protected set; }

    public virtual ChannelOrigin ChannelOrigin { get; protected set; }

    public virtual ChatSessionStatus Status { get; protected set; }

    public virtual string? MetadataJson { get; protected set; }

    public virtual DateTime? LastMessageTime { get; protected set; }

    protected ChatSession()
    {
    }

    public ChatSession(
        Guid id,
        Guid? tenantId,
        string? title,
        AccessMode accessMode,
        ConversationKind conversationKind,
        ChannelOrigin channelOrigin,
        string? metadataJson = null)
        : base(id)
    {
        TenantId = tenantId;
        AccessMode = accessMode;
        ConversationKind = conversationKind;
        ChannelOrigin = channelOrigin;
        Status = ChatSessionStatus.Open;
        SetTitle(title);
        SetMetadata(metadataJson);

        AddLocalEvent(new ChatSessionCreatedEvent(Id, TenantId));
    }

    public virtual void SetTitle(string? title)
    {
        Title = title.IsNullOrWhiteSpace()
            ? null
            : Check.Length(title, nameof(title), ChatConsts.MaxTitleLength);
    }

    public virtual void SetMetadata(string? metadataJson)
    {
        MetadataJson = metadataJson.IsNullOrWhiteSpace()
            ? null
            : Check.Length(metadataJson, nameof(metadataJson), ChatConsts.MaxMetadataJsonLength);
    }

    public virtual void MarkMessageReceived(Guid messageId, DateTime messageTime)
    {
        LastMessageTime = messageTime;
        AddLocalEvent(new ChatMessageSentEvent(messageId, Id, TenantId));
    }

    public virtual void EnsureOpen()
    {
        if (Status == ChatSessionStatus.Closed)
        {
            throw new BusinessException(ChatErrorCodes.SessionClosed)
                .WithData(nameof(Id), Id);
        }
    }

    public virtual void Close(Guid? closedByUserId = null)
    {
        if (Status == ChatSessionStatus.Closed)
        {
            return;
        }

        Status = ChatSessionStatus.Closed;
        AddLocalEvent(new ChatSessionClosedEvent(Id, TenantId, closedByUserId));
    }
}
