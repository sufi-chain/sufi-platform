using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserUpdateDto : IdentityUserCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    [DisableAuditing]
    [StringLength(256)]
    public string? Password { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
