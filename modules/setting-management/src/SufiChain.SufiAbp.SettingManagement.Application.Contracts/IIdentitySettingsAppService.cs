using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.SettingManagement;

public interface IIdentitySettingsAppService : IApplicationService
{
    Task<IdentitySettingsDto> GetAsync();

    Task UpdateAsync(UpdateIdentitySettingsDto input);
}
