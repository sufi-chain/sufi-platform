using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace SufiChain.SufiAbp.AspNetCore.Mvc.ApplicationConfigurations;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/application-configuration")]
public class SufiAbpApplicationConfigurationController : SufiAbpControllerBase, IAbpApplicationConfigurationAppService
{
    protected IAbpApplicationConfigurationAppService ApplicationConfigurationAppService { get; }

    protected IAbpAntiForgeryManager AntiForgeryManager { get; }

    public SufiAbpApplicationConfigurationController(
        IAbpApplicationConfigurationAppService applicationConfigurationAppService,
        IAbpAntiForgeryManager antiForgeryManager)
    {
        ApplicationConfigurationAppService = applicationConfigurationAppService;
        AntiForgeryManager = antiForgeryManager;
    }

    [HttpGet]
    public virtual async Task<ApplicationConfigurationDto> GetAsync(ApplicationConfigurationRequestOptions options)
    {
        AntiForgeryManager.SetCookie();
        return await ApplicationConfigurationAppService.GetAsync(options);
    }
}
