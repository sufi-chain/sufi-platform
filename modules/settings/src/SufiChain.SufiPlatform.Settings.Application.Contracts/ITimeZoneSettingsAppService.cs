using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Settings;

public interface ITimeZoneSettingsAppService : IApplicationService
{
    Task<List<NameValue>> GetTimezonesAsync();
    
    Task<TimeZoneSettingsDto> GetAsync();
    
    Task UpdateAsync(UpdateTimeZoneSettingsDto input);
}
