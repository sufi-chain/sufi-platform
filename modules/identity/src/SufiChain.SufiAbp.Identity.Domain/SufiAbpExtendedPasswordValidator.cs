using Microsoft.AspNetCore.Identity;
using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Identity;

public class SufiAbpExtendedPasswordValidator : IPasswordValidator<IdentityUser>, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public SufiAbpExtendedPasswordValidator(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return IdentityResult.Success;
        }

        var errors = new List<IdentityError>();

        if (await GetBoolAsync(IdentitySettingNames.Password.DisallowUsername) &&
            !string.IsNullOrEmpty(user.UserName) &&
            password.Contains(user.UserName, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new IdentityError { Code = "PasswordContainsUserName", Description = "Password must not contain the username." });
        }

        if (await GetBoolAsync(IdentitySettingNames.Password.DisallowEmail) &&
            !string.IsNullOrEmpty(user.Email))
        {
            var emailLocalPart = user.Email.Split('@')[0];
            if (!string.IsNullOrEmpty(emailLocalPart) &&
                password.Contains(emailLocalPart, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError { Code = "PasswordContainsEmail", Description = "Password must not contain the email address." });
            }
        }

        return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    }

    protected virtual async Task<bool> GetBoolAsync(string name, bool defaultValue = false)
    {
        var value = await SettingProvider.GetOrNullAsync(name);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
