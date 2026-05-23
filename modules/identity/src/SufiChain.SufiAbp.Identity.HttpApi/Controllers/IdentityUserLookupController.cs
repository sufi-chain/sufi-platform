using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using global::SufiChain.SufiAbp.Identity;
using Volo.Abp;

namespace SufiChain.SufiAbp.Identity.Controllers;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/identity/users/lookup")]
public class IdentityUserLookupController : SufiAbpControllerBase, IIdentityUserLookupAppService
{
    private readonly IIdentityUserLookupAppService _lookupAppService;

    public IdentityUserLookupController(IIdentityUserLookupAppService lookupAppService)
    {
        _lookupAppService = lookupAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<UserData> FindByIdAsync(Guid id)
    {
        return _lookupAppService.FindByIdAsync(id);
    }

    [HttpGet]
    [Route("by-username/{userName}")]
    public virtual Task<UserData> FindByUserNameAsync(string userName)
    {
        return _lookupAppService.FindByUserNameAsync(userName);
    }

    [HttpGet]
    [Route("search")]
    public virtual Task<ListResultDto<UserData>> SearchAsync(UserLookupSearchInputDto input)
    {
        return _lookupAppService.SearchAsync(input);
    }

    [HttpGet]
    [Route("count")]
    public virtual Task<long> GetCountAsync(UserLookupCountInputDto input)
    {
        return _lookupAppService.GetCountAsync(input);
    }
}
