using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base extensible DTO for creation audited entities.
/// </summary>
[Serializable]
public abstract class ExtensibleCreationAuditedEntityDto<TKey> : ExtensibleEntityDto<TKey>, ICreationAuditedObject
{
    public virtual DateTime CreationTime { get; set; }
    public virtual Guid? CreatorId { get; set; }
}
