using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Controllers;

[Area(SettingManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/setting-management/emailing")]
public class EmailSettingsController : SufiAbpControllerBase, IEmailSettingsAppService
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
