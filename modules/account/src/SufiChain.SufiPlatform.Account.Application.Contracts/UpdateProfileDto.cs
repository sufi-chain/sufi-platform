using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Account;

public class UpdateProfileDto : ExtensibleObject
{
    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
    public string UserName { get; set; }

    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
    public string Email { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxNameLength))]
    public string? Name { get; set; }

    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxSurnameLength))]
    public string? Surname { get; set; }

    // Length is enforced on the normalized number in ProfileUpdateRules.
    // A formatted value is longer than the 16-character column and must not fail binding.
    public string? PhoneNumber { get; set; }

    public string? ConcurrencyStamp { get; set; }
}
