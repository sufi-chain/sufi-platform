using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityRoleDto : ExtensibleEntityDto<Guid>, IHasConcurrencyStamp, IHasCreationTime
{
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsStatic { get; set; }

    public bool IsPublic { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }
}
