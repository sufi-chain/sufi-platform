using System.Collections.Generic;
using System.Threading.Tasks;
using AbpSettingDefinition = Volo.Abp.Settings.SettingDefinition;

namespace SufiChain.SufiPlatform.Settings;

public interface ISettingDefinitionSerializer
{
    Task<SettingDefinitionRecord> SerializeAsync(AbpSettingDefinition setting);

    Task<List<SettingDefinitionRecord>> SerializeAsync(IEnumerable<AbpSettingDefinition> settings);
}
