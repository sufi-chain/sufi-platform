using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.ConversationLinks;

public class ConversationLink : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid SessionId { get; protected set; }

    public virtual string LinkedEntityType { get; protected set; } = string.Empty;

    public virtual string LinkedEntityId { get; protected set; } = string.Empty;

    public virtual string? LinkRole { get; protected set; }

    public virtual string? MetadataJson { get; protected set; }

    protected ConversationLink()
    {
    }

    public ConversationLink(
        Guid id,
        Guid? tenantId,
        Guid sessionId,
        string linkedEntityType,
        string linkedEntityId,
        string? linkRole = null,
        string? metadataJson = null)
        : base(id)
    {
        TenantId = tenantId;
        SessionId = sessionId;
        SetLinkedEntity(linkedEntityType, linkedEntityId);
        SetLinkRole(linkRole);
        SetMetadata(metadataJson);
    }

    public virtual void SetLinkedEntity(string linkedEntityType, string linkedEntityId)
    {
        LinkedEntityType = Check.NotNullOrWhiteSpace(
            linkedEntityType,
            nameof(linkedEntityType),
            ChatConsts.MaxLinkedEntityTypeLength);

        LinkedEntityId = Check.NotNullOrWhiteSpace(
            linkedEntityId,
            nameof(linkedEntityId),
            ChatConsts.MaxLinkedEntityIdLength);
    }

    public virtual void SetLinkRole(string? linkRole)
    {
        LinkRole = linkRole.IsNullOrWhiteSpace()
            ? null
            : Check.Length(linkRole, nameof(linkRole), ChatConsts.MaxLinkRoleLength);
    }

    public virtual void SetMetadata(string? metadataJson)
    {
        MetadataJson = metadataJson.IsNullOrWhiteSpace()
            ? null
            : Check.Length(metadataJson, nameof(metadataJson), ChatConsts.MaxMetadataJsonLength);
    }
}
