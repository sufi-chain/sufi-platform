using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base extensible DTO for full audited entities with user navigation properties.
/// </summary>
[Serializable]
public abstract class ExtensibleFullAuditedEntityWithUserDto<TKey, TUserDto> : ExtensibleFullAuditedEntityDto<TKey>, IFullAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
    public virtual TUserDto? LastModifier { get; set; }
    public virtual TUserDto? Deleter { get; set; }
}
