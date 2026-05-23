using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for full audited entities with user navigation properties.
/// </summary>
[Serializable]
public abstract class FullAuditedEntityWithUserDto<TKey, TUserDto> : FullAuditedEntityDto<TKey>, IFullAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
    public virtual TUserDto? LastModifier { get; set; }
    public virtual TUserDto? Deleter { get; set; }
}
