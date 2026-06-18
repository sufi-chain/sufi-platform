using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.Controllers;

[Area(SettingManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SettingManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/setting-management/timezone")]
public class TimeZoneSettingsController : SufiAbpControllerBase, ITimeZoneSettingsAppService
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
