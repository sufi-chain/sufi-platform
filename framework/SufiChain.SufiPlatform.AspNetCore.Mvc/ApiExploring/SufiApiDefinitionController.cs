using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;
using Volo.Abp.Http.Modeling;

namespace SufiChain.SufiPlatform.AspNetCore.Mvc.ApiExploring;

[Area("sufi-abp")]
[RemoteService(Name = "sufi-abp")]
[Route("api/sufi-abp/api-definition")]
public class SufiApiDefinitionController : SufiControllerBase, IRemoteService
{
    protected IApiDescriptionModelProvider ModelProvider { get; }

    public SufiApiDefinitionController(IApiDescriptionModelProvider modelProvider)
    {
        ModelProvider = modelProvider;
    }

    [HttpGet]
    public virtual async Task<ApplicationApiDescriptionModel> GetAsync(ApplicationApiDescriptionModelRequestDto model)
    {
        return await ModelProvider.CreateApiModelAsync(model);
    }
}
