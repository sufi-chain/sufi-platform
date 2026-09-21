using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings;

[Authorize(SettingsPermissions.ExternalAuth)]
public class ExternalAuthSettingsAppService : SettingsAppServiceBase, IExternalAuthSettingsAppService
{
    protected ISettingManager SettingManager { get; }

    public ExternalAuthSettingsAppService(ISettingManager settingManager)
    {
        SettingManager = settingManager;
    }

    public virtual async Task<ExternalAuthSettingsDto> GetAsync()
    {
        await CheckFeatureAsync();

        return new ExternalAuthSettingsDto
        {
            Google = await GetProviderAsync(
                IdentitySettingNames.ExternalAuth.Google.Enabled,
                IdentitySettingNames.ExternalAuth.Google.ClientId,
                IdentitySettingNames.ExternalAuth.Google.ClientSecret),
            Microsoft = await GetProviderAsync(
                IdentitySettingNames.ExternalAuth.Microsoft.Enabled,
                IdentitySettingNames.ExternalAuth.Microsoft.ClientId,
                IdentitySettingNames.ExternalAuth.Microsoft.ClientSecret),
            GitHub = await GetProviderAsync(
                IdentitySettingNames.ExternalAuth.GitHub.Enabled,
                IdentitySettingNames.ExternalAuth.GitHub.ClientId,
                IdentitySettingNames.ExternalAuth.GitHub.ClientSecret)
        };
    }

    public virtual async Task UpdateAsync(UpdateExternalAuthSettingsDto input)
    {
        await CheckFeatureAsync();

        await SetProviderAsync(
            input.Google,
            IdentitySettingNames.ExternalAuth.Google.Enabled,
            IdentitySettingNames.ExternalAuth.Google.ClientId,
            IdentitySettingNames.ExternalAuth.Google.ClientSecret);
        await SetProviderAsync(
            input.Microsoft,
            IdentitySettingNames.ExternalAuth.Microsoft.Enabled,
            IdentitySettingNames.ExternalAuth.Microsoft.ClientId,
            IdentitySettingNames.ExternalAuth.Microsoft.ClientSecret);
        await SetProviderAsync(
            input.GitHub,
            IdentitySettingNames.ExternalAuth.GitHub.Enabled,
            IdentitySettingNames.ExternalAuth.GitHub.ClientId,
            IdentitySettingNames.ExternalAuth.GitHub.ClientSecret);
    }

    private async Task<ExternalAuthProviderSettingsDto> GetProviderAsync(
        string enabledName,
        string clientIdName,
        string clientSecretName)
    {
        var secret = await GetSettingValueOrNullAsync(clientSecretName);
        return new ExternalAuthProviderSettingsDto
        {
            Enabled = await GetBoolAsync(enabledName),
            ClientId = await GetStringAsync(clientIdName),
            HasClientSecret = !string.IsNullOrWhiteSpace(secret)
        };
    }

    private async Task SetProviderAsync(
        ExternalAuthProviderSettingsDto? input,
        string enabledName,
        string clientIdName,
        string clientSecretName)
    {
        input ??= new ExternalAuthProviderSettingsDto();
        await SettingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            enabledName,
            input.Enabled.ToString().ToLowerInvariant());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, clientIdName, input.ClientId);

        if (!string.IsNullOrWhiteSpace(input.ClientSecret))
        {
            await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, clientSecretName, input.ClientSecret);
        }
    }

    private async Task<string?> GetSettingValueOrNullAsync(string name)
    {
        return CurrentTenant.Id.HasValue
            ? await SettingManager.GetOrNullForTenantAsync(name, CurrentTenant.Id.Value, fallback: true)
            : await SettingManager.GetOrNullGlobalAsync(name);
    }

    private async Task<bool> GetBoolAsync(string name)
    {
        var value = await GetSettingValueOrNullAsync(name);
        return bool.TryParse(value, out var result) && result;
    }

    private async Task<string> GetStringAsync(string name)
    {
        return await GetSettingValueOrNullAsync(name) ?? string.Empty;
    }

    private async Task CheckFeatureAsync()
    {
        if (!await FeatureChecker.IsEnabledAsync(SettingsFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SettingsFeatures.Enable}");
        }
    }
}
