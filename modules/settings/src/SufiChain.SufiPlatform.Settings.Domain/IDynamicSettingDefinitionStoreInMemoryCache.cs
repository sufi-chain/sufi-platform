using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AbpSettingDefinition = Volo.Abp.Settings.SettingDefinition;

namespace SufiChain.SufiPlatform.Settings;

public interface IDynamicSettingDefinitionStoreInMemoryCache
{
    string CacheStamp { get; set; }

    SemaphoreSlim SyncSemaphore { get; }

    DateTime? LastCheckTime { get; set; }

    Task FillAsync(List<SettingDefinitionRecord> settingRecords);

    AbpSettingDefinition? GetSettingOrNull(string name);

    IReadOnlyList<AbpSettingDefinition> GetSettings();
}
