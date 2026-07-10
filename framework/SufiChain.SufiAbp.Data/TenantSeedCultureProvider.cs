using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Resolves tenant-specific default seed culture from settings with host fallback.
/// </summary>
public class TenantSeedCultureProvider : ITransientDependency
{
    /// <summary>
    /// Matches <see cref="LocalizationManagement.Settings.LocalizationManagementSettingNames.DefaultCulture"/>.
    /// </summary>
    public const string DefaultCultureSettingName = "Localization.DefaultCulture";

    protected ISettingProvider SettingProvider { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected SufiAbpDataSeedOptions DataSeedOptions { get; }

    public TenantSeedCultureProvider(
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        IOptions<SufiAbpDataSeedOptions> dataSeedOptions)
    {
        SettingProvider = settingProvider;
        CurrentTenant = currentTenant;
        DataSeedOptions = dataSeedOptions.Value;
    }

    public virtual async Task<string> GetDefaultCultureAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == null)
        {
            return SeedCultureHelper.NormalizeCulture(DataSeedOptions.DefaultCulture) ?? "fa";
        }

        using (CurrentTenant.Change(tenantId))
        {
            var tenantCulture = await SettingProvider.GetOrNullAsync(DefaultCultureSettingName);
            return SeedCultureHelper.NormalizeCulture(tenantCulture)
                ?? SeedCultureHelper.NormalizeCulture(DataSeedOptions.DefaultCulture)
                ?? "fa";
        }
    }
}
