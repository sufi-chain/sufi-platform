using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for full audited entities (creation, modification, deletion tracking).
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
[Serializable]
public abstract class FullAuditedEntityDto<TKey> : AuditedEntityDto<TKey>, IFullAuditedObject
{
    public virtual bool IsDeleted { get; set; }
    public virtual Guid? DeleterId { get; set; }
    public virtual DateTime? DeletionTime { get; set; }
}
