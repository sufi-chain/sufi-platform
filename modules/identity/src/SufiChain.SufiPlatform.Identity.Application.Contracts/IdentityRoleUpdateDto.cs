using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityRoleUpdateDto : IdentityRoleCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
