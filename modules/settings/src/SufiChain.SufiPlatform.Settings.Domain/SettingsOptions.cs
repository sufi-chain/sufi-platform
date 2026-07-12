using Volo.Abp.Collections;

namespace SufiChain.SufiPlatform.Settings;

public class SettingsOptions
{
    public ITypeList<ISettingsProvider> Providers { get; }

    /// <summary>
    /// Default: true.
    /// </summary>
    public bool SaveStaticSettingsToDatabase { get; set; } = true;

    /// <summary>
    /// Default: false.
    /// </summary>
    public bool IsDynamicSettingStoreEnabled { get; set; }

    public SettingsOptions()
    {
        Providers = new TypeList<ISettingsProvider>();
    }
}
