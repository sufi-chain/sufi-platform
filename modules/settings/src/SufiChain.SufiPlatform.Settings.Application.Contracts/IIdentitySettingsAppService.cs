using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Settings;

public interface IIdentitySettingsAppService : IApplicationService
{
    Task<IdentitySettingsDto> GetAsync();

    Task UpdateAsync(UpdateIdentitySettingsDto input);
}
