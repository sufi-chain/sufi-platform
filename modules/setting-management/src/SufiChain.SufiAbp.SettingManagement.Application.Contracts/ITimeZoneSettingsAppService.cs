using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.SettingManagement;

public interface ITimeZoneSettingsAppService : IApplicationService
{
    Task<List<NameValue>> GetTimezonesAsync();
    
    Task<TimeZoneSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateTimeZoneSettingsDto input);
}
