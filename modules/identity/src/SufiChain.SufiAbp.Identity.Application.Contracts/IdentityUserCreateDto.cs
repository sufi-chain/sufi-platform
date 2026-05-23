using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserCreateDto : IdentityUserCreateOrUpdateDtoBase
{
    [DisableAuditing]
    [Required]
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
