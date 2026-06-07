using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Messages;

public class ChatMessage : FullAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid SessionId { get; protected set; }

    public virtual string Body { get; protected set; } = string.Empty;

    public virtual ChatMessageSenderKind SenderKind { get; protected set; }

    public virtual Guid? SenderUserId { get; protected set; }

    public virtual string? AnonymousVisitorId { get; protected set; }

    public virtual bool IsInternal { get; protected set; }

    public virtual string? MetadataJson { get; protected set; }

    public virtual List<Guid> AttachmentFileIds { get; protected set; } = new();

    protected ChatMessage()
    {
    }

    public ChatMessage(
        Guid id,
        Guid? tenantId,
        Guid sessionId,
        string body,
        ChatMessageSenderKind senderKind,
        Guid? senderUserId = null,
        string? anonymousVisitorId = null,
        bool isInternal = false,
        string? metadataJson = null,
        IEnumerable<Guid>? attachmentFileIds = null)
        : base(id)
    {
        TenantId = tenantId;
        SessionId = sessionId;
        SenderKind = senderKind;
        SenderUserId = senderUserId;
        AnonymousVisitorId = anonymousVisitorId.IsNullOrWhiteSpace()
            ? null
            : Check.Length(anonymousVisitorId, nameof(anonymousVisitorId), ChatConsts.MaxAnonymousVisitorIdLength);
        IsInternal = isInternal;
        SetBody(body);
        SetMetadata(metadataJson);

        if (attachmentFileIds != null)
        {
            AttachmentFileIds.AddRange(attachmentFileIds);
        }
    }

    public virtual void SetBody(string body)
    {
        body ??= string.Empty;

        if (string.IsNullOrWhiteSpace(body))
        {
            Body = string.Empty;
            return;
        }

        Body = Check.Length(body.Trim(), nameof(body), ChatConsts.MaxMessageBodyLength);
    }

    public virtual void SetMetadata(string? metadataJson)
    {
        MetadataJson = metadataJson.IsNullOrWhiteSpace()
            ? null
            : Check.Length(metadataJson, nameof(metadataJson), ChatConsts.MaxMetadataJsonLength);
    }
}
