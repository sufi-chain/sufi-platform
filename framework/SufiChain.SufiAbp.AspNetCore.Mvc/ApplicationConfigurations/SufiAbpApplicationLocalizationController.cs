using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace SufiChain.SufiAbp.AspNetCore.Mvc.ApplicationConfigurations;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/application-localization")]
public class SufiAbpApplicationLocalizationController : SufiAbpControllerBase, IAbpApplicationLocalizationAppService
{
    protected IAbpApplicationLocalizationAppService LocalizationAppService { get; }

    public SufiAbpApplicationLocalizationController(IAbpApplicationLocalizationAppService localizationAppService)
    {
        LocalizationAppService = localizationAppService;
    }

    [HttpGet]
    public virtual async Task<ApplicationLocalizationDto> GetAsync(ApplicationLocalizationRequestDto input)
    {
        return await LocalizationAppService.GetAsync(input);
    }
}
