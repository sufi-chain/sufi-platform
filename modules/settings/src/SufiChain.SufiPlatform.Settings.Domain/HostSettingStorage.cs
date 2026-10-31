using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public static class HostSettingStorage
{
    public static SettingDefinition RestrictToHost(this SettingDefinition definition)
    {
        definition.Providers.RemoveAll(provider =>
            provider == TenantSettingValueProvider.ProviderName ||
            provider == UserSettingValueProvider.ProviderName);

        Add(definition, DefaultValueSettingValueProvider.ProviderName);
        Add(definition, ConfigurationSettingValueProvider.ProviderName);
        Add(definition, GlobalSettingValueProvider.ProviderName);
        return definition;
    }

    private static void Add(SettingDefinition definition, string providerName)
    {
        if (!definition.Providers.Contains(providerName))
        {
            definition.Providers.Add(providerName);
        }
    }
}
