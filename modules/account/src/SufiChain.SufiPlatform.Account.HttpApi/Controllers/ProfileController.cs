using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Account.Controllers;

[Area(AccountRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AccountRemoteServiceConsts.RemoteServiceName)]
[Route("api/account/my-profile")]
public class ProfileController : SufiControllerBase, IProfileAppService
{
    private readonly IProfileAppService _profileAppService;

    public ProfileController(IProfileAppService profileAppService)
    {
        _profileAppService = profileAppService;
    }

    [HttpGet]
    public virtual Task<ProfileDto> GetAsync()
    {
        return _profileAppService.GetAsync();
    }

    [HttpPut]
    public virtual Task<ProfileDto> UpdateAsync(UpdateProfileDto input)
    {
        return _profileAppService.UpdateAsync(input);
    }

    [HttpPost]
    [Route("change-password")]
    public virtual Task ChangePasswordAsync(ChangePasswordInput input)
    {
        return _profileAppService.ChangePasswordAsync(input);
    }
}
