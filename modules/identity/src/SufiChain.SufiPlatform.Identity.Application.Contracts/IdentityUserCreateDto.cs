using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityUserCreateDto : IdentityUserCreateOrUpdateDtoBase
{
    [DisableAuditing]
    [Required]
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;
}
