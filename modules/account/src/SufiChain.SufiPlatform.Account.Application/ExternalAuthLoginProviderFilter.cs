using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

public class ExternalAuthLoginProviderFilter : IExternalAuthLoginProviderFilter, ITransientDependency
{
    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;

    public ExternalAuthLoginProviderFilter(ISettingProvider settingProvider, IConfiguration configuration)
    {
        _settingProvider = settingProvider;
        _configuration = configuration;
    }

    public virtual async Task<IReadOnlyList<string>> FilterEnabledAsync(IEnumerable<string> schemeNames)
    {
        var enabled = new List<string>();
        foreach (var name in schemeNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            if (await IsEnabledAsync(name))
            {
                enabled.Add(name);
            }
        }

        return enabled;
    }

    private async Task<bool> IsEnabledAsync(string schemeName)
    {
        if (schemeName.Equals("Google", StringComparison.OrdinalIgnoreCase))
        {
            return await IsManagedProviderEnabledAsync(
                IdentitySettingNames.ExternalAuth.Google.Enabled,
                IdentitySettingNames.ExternalAuth.Google.ClientId,
                "ExternalAuth:Google:ClientId");
        }

        if (schemeName.Equals("Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            return await IsManagedProviderEnabledAsync(
                IdentitySettingNames.ExternalAuth.Microsoft.Enabled,
                IdentitySettingNames.ExternalAuth.Microsoft.ClientId,
                "ExternalAuth:Microsoft:ClientId");
        }

        if (schemeName.Equals("GitHub", StringComparison.OrdinalIgnoreCase))
        {
            return await IsManagedProviderEnabledAsync(
                IdentitySettingNames.ExternalAuth.GitHub.Enabled,
                IdentitySettingNames.ExternalAuth.GitHub.ClientId,
                "ExternalAuth:GitHub:ClientId");
        }

        return true;
    }

    private async Task<bool> IsManagedProviderEnabledAsync(
        string enabledSetting,
        string clientIdSetting,
        string configurationClientId)
    {
        var settingClientId = await _settingProvider.GetOrNullAsync(clientIdSetting);
        var configClientId = _configuration[configurationClientId];
        var clientId = string.IsNullOrWhiteSpace(settingClientId) ? configClientId : settingClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return false;
        }

        var enabled = bool.TryParse(await _settingProvider.GetOrNullAsync(enabledSetting), out var flag) && flag;
        return enabled || !string.IsNullOrWhiteSpace(configClientId);
    }
}
