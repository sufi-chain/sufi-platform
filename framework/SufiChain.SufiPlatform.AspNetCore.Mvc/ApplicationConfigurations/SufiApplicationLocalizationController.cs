using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;

namespace SufiChain.SufiPlatform.AspNetCore.Mvc.ApplicationConfigurations;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/application-localization")]
public class SufiApplicationLocalizationController : SufiControllerBase, IAbpApplicationLocalizationAppService
{
    protected IAbpApplicationLocalizationAppService LocalizationAppService { get; }

    public SufiApplicationLocalizationController(IAbpApplicationLocalizationAppService localizationAppService)
    {
        LocalizationAppService = localizationAppService;
    }

    [HttpGet]
    public virtual async Task<ApplicationLocalizationDto> GetAsync(ApplicationLocalizationRequestDto input)
    {
        return await LocalizationAppService.GetAsync(input);
    }
}
