using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.AiUsage;

public interface IChatAiUsageAppService : IApplicationService
{
    Task<ChatAiUsageDashboardDto> GetDashboardAsync(GetChatAiUsageDashboardInput input);

    Task<PagedResultDto<ChatAiUsageReservationDto>> GetReservationsAsync(GetChatAiUsageReservationsInput input);

    Task<PagedResultDto<ChatAiUsageRecordDto>> GetUsageRecordsAsync(GetChatAiUsageRecordsInput input);

    Task<ChatAiUsagePolicyDto> GetEffectivePolicyAsync(Guid? sessionId = null);
}
