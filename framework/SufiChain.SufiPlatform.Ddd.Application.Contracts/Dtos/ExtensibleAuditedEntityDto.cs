using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Application.Dtos;

/// <summary>
/// Base extensible DTO for audited entities with creation and modification tracking.
/// </summary>
[Serializable]
public abstract class ExtensibleAuditedEntityDto<TKey> : ExtensibleCreationAuditedEntityDto<TKey>, IAuditedObject
{
    public virtual DateTime? LastModificationTime { get; set; }
    public virtual Guid? LastModifierId { get; set; }
}
