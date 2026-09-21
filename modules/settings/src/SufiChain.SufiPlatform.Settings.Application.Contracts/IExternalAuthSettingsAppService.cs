using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Settings;

public interface IExternalAuthSettingsAppService
{
    Task<ExternalAuthSettingsDto> GetAsync();

    Task UpdateAsync(UpdateExternalAuthSettingsDto input);
}
