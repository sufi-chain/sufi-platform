using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Settings;

public class StaticSettingSaver : IStaticSettingSaver, ITransientDependency
{
    public ILogger<StaticSettingSaver> Logger { get; set; }
    
    protected ISettingDefinitionManager SettingDefinitionManager { get; }
    protected ISettingsStore SettingsStore { get; }

    public StaticSettingSaver(
        ISettingDefinitionManager settingDefinitionManager,
        ISettingsStore settingManagementStore)
    {
        Logger = NullLogger<StaticSettingSaver>.Instance;
        SettingDefinitionManager = settingDefinitionManager;
        SettingsStore = settingManagementStore;
    }

    public virtual async Task SaveAsync()
    {
        var settingDefinitions = await SettingDefinitionManager.GetAllAsync();
        
        foreach (var settingDefinition in settingDefinitions)
        {
            if (!string.IsNullOrEmpty(settingDefinition.DefaultValue))
            {
                // Only save default if setting doesn't already exist in database
                var existingValue = await SettingsStore.GetOrNullAsync(
                    settingDefinition.Name,
                    GlobalSettingValueProvider.ProviderName,
                    string.Empty);
                
                if (existingValue == null)
                {
                    await SettingsStore.SetAsync(
                        settingDefinition.Name,
                        settingDefinition.DefaultValue,
                        GlobalSettingValueProvider.ProviderName,
                        string.Empty
                    );
                }
            }
        }
    }
}
