using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Options;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

public class SufiIdentityOptionsManager : AbpDynamicOptionsManager<IdentityOptions>, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public SufiIdentityOptionsManager(
        IOptionsFactory<IdentityOptions> factory,
        ISettingProvider settingProvider)
        : base(factory)
    {
        SettingProvider = settingProvider;
    }

    protected override async Task OverrideOptionsAsync(string name, IdentityOptions options)
    {
        options.SignIn.RequireConfirmedEmail = await GetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail);
        options.SignIn.RequireConfirmedPhoneNumber = await GetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber);
        options.User.RequireUniqueEmail = await GetBoolAsync(IdentitySettingNames.User.RequireUniqueEmail);
        options.Password.RequiredLength = await GetIntAsync(IdentitySettingNames.Password.RequiredLength, options.Password.RequiredLength);
        options.Password.RequiredUniqueChars = await GetIntAsync(IdentitySettingNames.Password.RequiredUniqueChars, options.Password.RequiredUniqueChars);
        options.Password.RequireNonAlphanumeric = await GetBoolAsync(IdentitySettingNames.Password.RequireNonAlphanumeric, options.Password.RequireNonAlphanumeric);
        options.Password.RequireLowercase = await GetBoolAsync(IdentitySettingNames.Password.RequireLowercase, options.Password.RequireLowercase);
        options.Password.RequireUppercase = await GetBoolAsync(IdentitySettingNames.Password.RequireUppercase, options.Password.RequireUppercase);
        options.Password.RequireDigit = await GetBoolAsync(IdentitySettingNames.Password.RequireDigit, options.Password.RequireDigit);
        options.Lockout.AllowedForNewUsers = await GetBoolAsync(IdentitySettingNames.Lockout.AllowedForNewUsers, options.Lockout.AllowedForNewUsers);
        options.Lockout.MaxFailedAccessAttempts = await GetIntAsync(IdentitySettingNames.Lockout.MaxFailedAccessAttempts, options.Lockout.MaxFailedAccessAttempts);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
            await GetIntAsync(IdentitySettingNames.Lockout.DefaultLockoutTimeSpanMinutes, (int)options.Lockout.DefaultLockoutTimeSpan.TotalMinutes));
    }

    protected virtual async Task<bool> GetBoolAsync(string name, bool defaultValue = false)
    {
        var value = await SettingProvider.GetOrNullAsync(name);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    protected virtual async Task<int> GetIntAsync(string name, int defaultValue)
    {
        var value = await SettingProvider.GetOrNullAsync(name);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}
