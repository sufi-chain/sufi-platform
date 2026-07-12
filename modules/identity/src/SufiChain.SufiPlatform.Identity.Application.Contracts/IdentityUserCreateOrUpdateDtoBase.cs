using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiPlatform.Identity;

public abstract class IdentityUserCreateOrUpdateDtoBase : ExtensibleObject
{
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = string.Empty;

    [StringLength(64)]
    public string? Name { get; set; }

    [StringLength(64)]
    public string? Surname { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(16)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public bool LockoutEnabled { get; set; }

    [CanBeNull]
    public string[]? RoleNames { get; set; }

    protected IdentityUserCreateOrUpdateDtoBase()
        : base(false)
    {
    }
}
