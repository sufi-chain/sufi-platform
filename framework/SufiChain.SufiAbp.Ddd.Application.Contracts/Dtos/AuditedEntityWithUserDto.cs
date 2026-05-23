using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base DTO for audited entities with user navigation properties.
/// </summary>
[Serializable]
public abstract class AuditedEntityWithUserDto<TKey, TUserDto> : AuditedEntityDto<TKey>, IAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
    public virtual TUserDto? LastModifier { get; set; }
}
