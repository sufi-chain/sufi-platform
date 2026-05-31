using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Identity.AspNetCore;

public class IdentityTokenOptionsConfigurator : IConfigureOptions<DataProtectionTokenProviderOptions>, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public IdentityTokenOptionsConfigurator(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public void Configure(DataProtectionTokenProviderOptions options)
    {
        ConfigureAsync(options).GetAwaiter().GetResult();
    }

    protected virtual async Task ConfigureAsync(DataProtectionTokenProviderOptions options)
    {
        var emailHours = await GetIntAsync(IdentitySettingNames.Tokens.EmailConfirmationTokenLifespanHours, 24);
        options.TokenLifespan = TimeSpan.FromHours(emailHours);
    }

    protected virtual async Task<int> GetIntAsync(string name, int defaultValue)
    {
        var value = await SettingProvider.GetOrNullAsync(name);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}
