using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base extensible DTO for full audited entities (creation, modification, deletion tracking).
/// </summary>
[Serializable]
public abstract class ExtensibleFullAuditedEntityDto<TKey> : ExtensibleAuditedEntityDto<TKey>, IFullAuditedObject
{
    public virtual bool IsDeleted { get; set; }
    public virtual Guid? DeleterId { get; set; }
    public virtual DateTime? DeletionTime { get; set; }
}
