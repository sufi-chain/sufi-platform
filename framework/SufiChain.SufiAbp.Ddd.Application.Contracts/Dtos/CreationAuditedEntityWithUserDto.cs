using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for creation audited entities with creator navigation.
/// </summary>
[Serializable]
public abstract class CreationAuditedEntityWithUserDto<TKey, TUserDto> : CreationAuditedEntityDto<TKey>, ICreationAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
}
