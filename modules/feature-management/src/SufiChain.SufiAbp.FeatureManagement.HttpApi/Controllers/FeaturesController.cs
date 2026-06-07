using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.FeatureManagement;

namespace SufiChain.SufiAbp.FeatureManagement.Controllers;

[Area(FeatureManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = FeatureManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/feature-management/features")]
public class FeaturesController : SufiAbpControllerBase, IFeatureAppService
{
    private readonly IFeatureAppService _featureAppService;

    public FeaturesController(IFeatureAppService featureAppService)
    {
        _featureAppService = featureAppService;
    }

    [HttpGet]
    public virtual Task<GetFeatureListResultDto> GetAsync(string providerName, string? providerKey)
    {
        return _featureAppService.GetAsync(providerName, providerKey);
    }

    [HttpPut]
    public virtual Task UpdateAsync(string providerName, string? providerKey, UpdateFeaturesDto input)
    {
        return _featureAppService.UpdateAsync(providerName, providerKey, input);
    }

    [HttpDelete]
    public virtual Task DeleteAsync(string providerName, string? providerKey)
    {
        return _featureAppService.DeleteAsync(providerName, providerKey);
    }
}
