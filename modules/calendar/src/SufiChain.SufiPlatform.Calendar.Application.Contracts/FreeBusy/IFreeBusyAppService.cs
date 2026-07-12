using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public interface IFreeBusyAppService : IApplicationService
{
    Task<FreeBusyResultDto> GetFreeBusyAsync(GetFreeBusyInput input);

    Task<ListResultDto<FreeBusySlotDto>> FindAvailableSlotsAsync(FindAvailableSlotsInput input);
}
