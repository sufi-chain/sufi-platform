using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.SettingManagement;

public interface IEmailSettingsAppService : IApplicationService
{
    Task<EmailSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateEmailSettingsDto input);
    
    Task SendTestEmailAsync(SendTestEmailInput input);
}
