using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Auditing;
using Volo.Abp.Validation;

namespace SufiChain.SufiAbp.Account;

public class ChangePasswordInput : IValidatableObject
{
    [DisableAuditing]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    public string CurrentPassword { get; set; }

    [Required]
    [DisableAuditing]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    public string NewPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentPassword == NewPassword) 
        {
            var localizer = validationContext.GetRequiredService<IStringLocalizer<SufiAbpAccountResource>>();

            yield return new ValidationResult(
                localizer["NewPasswordSameAsOld"],
                new[] { nameof(CurrentPassword), nameof(NewPassword) }
            );
        }
    }
}
