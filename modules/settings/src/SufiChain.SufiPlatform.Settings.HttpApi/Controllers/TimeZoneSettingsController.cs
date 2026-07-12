using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.Controllers;

[Area(SettingsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingsRemoteServiceConsts.RemoteServiceName)]
[Route("api/settings/timezone")]
public class TimeZoneSettingsController : SufiControllerBase, ITimeZoneSettingsAppService
{
    private readonly ITimeZoneSettingsAppService _timeZoneSettingsAppService;

    public TimeZoneSettingsController(ITimeZoneSettingsAppService timeZoneSettingsAppService)
    {
        _timeZoneSettingsAppService = timeZoneSettingsAppService;
    }

    [HttpGet]
    public virtual Task<TimeZoneSettingsDto> GetAsync()
    {
        return _timeZoneSettingsAppService.GetAsync();
    }

    [HttpGet]
    [Route("timezones")]
    public virtual Task<List<NameValue>> GetTimezonesAsync()
    {
        return _timeZoneSettingsAppService.GetTimezonesAsync();
    }

    [HttpPost]
    public virtual Task UpdateAsync(UpdateTimeZoneSettingsDto input)
    {
        return _timeZoneSettingsAppService.UpdateAsync(input);
    }
}
