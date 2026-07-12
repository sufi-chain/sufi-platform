using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Controllers;

[Area(SettingsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingsRemoteServiceConsts.RemoteServiceName)]
[Route("api/settings/emailing")]
public class EmailSettingsController : SufiControllerBase, IEmailSettingsAppService
{
    private readonly IEmailSettingsAppService _emailSettingsAppService;

    public EmailSettingsController(IEmailSettingsAppService emailSettingsAppService)
    {
        _emailSettingsAppService = emailSettingsAppService;
    }

    [HttpGet]
    public virtual Task<EmailSettingsDto> GetAsync()
    {
        return _emailSettingsAppService.GetAsync();
    }

    [HttpPost]
    public virtual Task UpdateAsync(UpdateEmailSettingsDto input)
    {
        return _emailSettingsAppService.UpdateAsync(input);
    }

    [HttpPost]
    [Route("send-test-email")]
    public virtual Task SendTestEmailAsync(SendTestEmailInput input)
    {
        return _emailSettingsAppService.SendTestEmailAsync(input);
    }
}
