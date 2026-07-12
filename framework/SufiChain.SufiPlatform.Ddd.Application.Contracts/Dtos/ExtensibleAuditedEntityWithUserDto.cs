using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Application.Dtos;

/// <summary>
/// Base extensible DTO for audited entities with user navigation properties.
/// </summary>
[Serializable]
public abstract class ExtensibleAuditedEntityWithUserDto<TKey, TUserDto> : ExtensibleAuditedEntityDto<TKey>, IAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
    public virtual TUserDto? LastModifier { get; set; }
}
