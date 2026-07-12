using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace SufiChain.SufiPlatform.AspNetCore.Mvc.ApplicationConfigurations;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/application-configuration")]
public class SufiApplicationConfigurationController : SufiControllerBase, IAbpApplicationConfigurationAppService
{
    protected IAbpApplicationConfigurationAppService ApplicationConfigurationAppService { get; }

    protected IAbpAntiForgeryManager AntiForgeryManager { get; }

    public SufiApplicationConfigurationController(
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
