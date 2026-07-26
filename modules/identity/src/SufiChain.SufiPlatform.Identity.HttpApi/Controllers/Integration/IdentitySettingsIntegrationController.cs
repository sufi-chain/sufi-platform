using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Identity.Integration;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Identity.Controllers.Integration;

/// <summary>
/// HTTP API for Identity settings IntegrationService.
/// </summary>
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Area(IdentityRemoteServiceConsts.ModuleName)]
[ControllerName("IdentitySettingsIntegration")]
[Route("integration-api/identity/settings")]
public class IdentitySettingsIntegrationController : SufiControllerBase, IIdentitySettingsIntegrationService
{
    protected IIdentitySettingsIntegrationService SettingsIntegrationService { get; }

    public IdentitySettingsIntegrationController(IIdentitySettingsIntegrationService settingsIntegrationService)
    {
        SettingsIntegrationService = settingsIntegrationService;
    }

    [HttpGet]
    [Route("password-requirements")]
    public virtual Task<IdentityPasswordRequirementsDto> GetPasswordRequirementsAsync()
    {
        return SettingsIntegrationService.GetPasswordRequirementsAsync();
    }
}
