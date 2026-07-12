using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Application.Dtos;

/// <summary>
/// Base extensible DTO for creation audited entities with creator navigation.
/// </summary>
[Serializable]
public abstract class ExtensibleCreationAuditedEntityWithUserDto<TKey, TUserDto> : ExtensibleCreationAuditedEntityDto<TKey>, ICreationAuditedObject
{
    public virtual TUserDto? Creator { get; set; }
}
