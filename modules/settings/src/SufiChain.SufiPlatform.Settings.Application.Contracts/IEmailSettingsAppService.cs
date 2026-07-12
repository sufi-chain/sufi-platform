using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Settings;

public interface IEmailSettingsAppService : IApplicationService
{
    Task<EmailSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateEmailSettingsDto input);
    
    Task SendTestEmailAsync(SendTestEmailInput input);
}
