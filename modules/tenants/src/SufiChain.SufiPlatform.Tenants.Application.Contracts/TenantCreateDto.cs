using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantCreateDto : TenantCreateOrUpdateDtoBase
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    [Display(Name = "Field:AdminEmailAddress")]
    public virtual string AdminEmailAddress { get; set; }

    [Required]
    [MaxLength(128)]
    [DisableAuditing]
    [Display(Name = "Field:AdminPassword")]
    public virtual string AdminPassword { get; set; }
}
