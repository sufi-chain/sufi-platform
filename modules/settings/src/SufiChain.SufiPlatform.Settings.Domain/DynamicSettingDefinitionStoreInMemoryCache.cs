using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using AbpSettingDefinition = Volo.Abp.Settings.SettingDefinition;

namespace SufiChain.SufiPlatform.Settings;

public class DynamicSettingDefinitionStoreInMemoryCache :
    IDynamicSettingDefinitionStoreInMemoryCache,
    ISingletonDependency
{
    public string CacheStamp { get; set; } = default!;

    protected IDictionary<string, AbpSettingDefinition> SettingDefinitions { get; }
    protected ILocalizableStringSerializer LocalizableStringSerializer { get; }

    public SemaphoreSlim SyncSemaphore { get; } = new(1, 1);

    public DateTime? LastCheckTime { get; set; }

    public DynamicSettingDefinitionStoreInMemoryCache(ILocalizableStringSerializer localizableStringSerializer)
    {
        LocalizableStringSerializer = localizableStringSerializer;
        SettingDefinitions = new Dictionary<string, AbpSettingDefinition>();
    }

    public Task FillAsync(List<SettingDefinitionRecord> settingRecords)
    {
        SettingDefinitions.Clear();

        foreach (var record in settingRecords)
        {
            var settingDefinition = new AbpSettingDefinition(
                record.Name,
                record.DefaultValue,
                LocalizableStringSerializer.Deserialize(record.DisplayName),
                record.Description != null
                    ? LocalizableStringSerializer.Deserialize(record.Description)
                    : null,
                record.IsVisibleToClients,
                record.IsInherited,
                record.IsEncrypted);

            if (!record.Providers.IsNullOrWhiteSpace())
            {
                settingDefinition.Providers.AddRange(record.Providers.Split(','));
            }

            foreach (var property in record.ExtraProperties)
            {
                settingDefinition.WithProperty(property.Key, property.Value);
            }

            SettingDefinitions[record.Name] = settingDefinition;
        }

        return Task.CompletedTask;
    }

    public AbpSettingDefinition? GetSettingOrNull(string name)
    {
        return SettingDefinitions.GetOrDefault(name);
    }

    public IReadOnlyList<AbpSettingDefinition> GetSettings()
    {
        return SettingDefinitions.Values.ToList();
    }
}
