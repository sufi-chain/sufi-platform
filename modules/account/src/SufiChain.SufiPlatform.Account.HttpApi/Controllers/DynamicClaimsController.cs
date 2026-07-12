using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/account/dynamic-claims")]
public class DynamicClaimsController : SufiControllerBase, IDynamicClaimsAppService
{
    private readonly IDynamicClaimsAppService _dynamicClaimsAppService;

    public DynamicClaimsController(IDynamicClaimsAppService dynamicClaimsAppService)
    {
        _dynamicClaimsAppService = dynamicClaimsAppService;
    }

    [HttpPost]
    [Route("refresh")]
    public virtual Task RefreshAsync()
    {
        return _dynamicClaimsAppService.RefreshAsync();
    }
}
