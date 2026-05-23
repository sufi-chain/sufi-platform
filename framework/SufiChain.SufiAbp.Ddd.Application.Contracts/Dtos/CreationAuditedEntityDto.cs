using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for creation audited entities.
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
[Serializable]
public abstract class CreationAuditedEntityDto<TKey> : SufiAbpEntityDto<TKey>, ICreationAuditedObject
{
    public virtual DateTime CreationTime { get; set; }
    public virtual Guid? CreatorId { get; set; }
}
