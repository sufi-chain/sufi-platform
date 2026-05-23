using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for audited entities with creation and modification tracking.
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
[Serializable]
public abstract class AuditedEntityDto<TKey> : CreationAuditedEntityDto<TKey>, IAuditedObject
{
    public virtual DateTime? LastModificationTime { get; set; }
    public virtual Guid? LastModifierId { get; set; }
}
