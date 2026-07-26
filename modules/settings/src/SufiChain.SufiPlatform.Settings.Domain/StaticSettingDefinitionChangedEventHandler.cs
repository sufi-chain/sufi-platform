using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Settings;
using Volo.Abp.StaticDefinitions;
using Volo.Abp.Threading;
using AbpSettingDefinition = Volo.Abp.Settings.SettingDefinition;

namespace SufiChain.SufiPlatform.Settings;

public class StaticSettingDefinitionChangedEventHandler :
    ILocalEventHandler<StaticSettingDefinitionChangedEvent>,
    ITransientDependency
{
    protected IStaticDefinitionCache<AbpSettingDefinition, Dictionary<string, AbpSettingDefinition>> DefinitionCache { get; }
    protected SettingDynamicInitializer SettingDynamicInitializer { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }

    public StaticSettingDefinitionChangedEventHandler(
        IStaticDefinitionCache<AbpSettingDefinition, Dictionary<string, AbpSettingDefinition>> definitionCache,
        SettingDynamicInitializer settingDynamicInitializer,
        ICancellationTokenProvider cancellationTokenProvider)
    {
        DefinitionCache = definitionCache;
        SettingDynamicInitializer = settingDynamicInitializer;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    public virtual async Task HandleEventAsync(StaticSettingDefinitionChangedEvent eventData)
    {
        await DefinitionCache.ClearAsync();
        await SettingDynamicInitializer.InitializeAsync(false, CancellationTokenProvider.Token);
    }
}
