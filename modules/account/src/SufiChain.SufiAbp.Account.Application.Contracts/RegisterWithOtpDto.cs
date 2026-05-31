using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Auditing;
using Volo.Abp.Validation;

namespace SufiChain.SufiAbp.Account;

public class RegisterWithOtpDto
{
    [Required]
    public string RegistrationToken { get; set; } = string.Empty;

    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
    public string EmailAddress { get; set; } = string.Empty;

    [Required]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    [DataType(DataType.Password)]
    [DisableAuditing]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string AppName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }
}
