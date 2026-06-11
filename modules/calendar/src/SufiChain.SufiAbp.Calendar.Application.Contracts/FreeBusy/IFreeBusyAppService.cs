using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public interface IFreeBusyAppService : IApplicationService
{
    Task<FreeBusyResultDto> GetFreeBusyAsync(GetFreeBusyInput input);

    Task<ListResultDto<FreeBusySlotDto>> FindAvailableSlotsAsync(FindAvailableSlotsInput input);
}
