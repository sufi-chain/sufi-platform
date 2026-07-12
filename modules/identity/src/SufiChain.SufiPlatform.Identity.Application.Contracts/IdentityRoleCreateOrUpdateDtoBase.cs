using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityRoleCreateOrUpdateDtoBase : ExtensibleObject
{
    [Required]
    [StringLength(256)]
    [Display(Name = "RoleName")]
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsPublic { get; set; }

    protected IdentityRoleCreateOrUpdateDtoBase()
        : base(false)
    {
    }
}
