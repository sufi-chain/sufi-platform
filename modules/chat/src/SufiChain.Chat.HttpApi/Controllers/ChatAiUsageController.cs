using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Permissions;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[Route("api/admin/chat/ai")]
public class ChatAiUsageController : ChatController, IChatAiUsageAppService
{
    private readonly IChatAiUsageAppService _aiUsageAppService;

    public ChatAiUsageController(IChatAiUsageAppService aiUsageAppService)
    {
        _aiUsageAppService = aiUsageAppService;
    }

    [HttpGet("dashboard")]
    [Authorize(ChatPermissions.AiUsage.View)]
    public virtual Task<ChatAiUsageDashboardDto> GetDashboardAsync([FromQuery] GetChatAiUsageDashboardInput input)
    {
        return _aiUsageAppService.GetDashboardAsync(input);
    }

    [HttpGet("reservations")]
    [Authorize(ChatPermissions.AiUsage.View)]
    public virtual Task<PagedResultDto<ChatAiUsageReservationDto>> GetReservationsAsync([FromQuery] GetChatAiUsageReservationsInput input)
    {
        return _aiUsageAppService.GetReservationsAsync(input);
    }

    [HttpGet("records")]
    [Authorize(ChatPermissions.AiUsage.View)]
    public virtual Task<PagedResultDto<ChatAiUsageRecordDto>> GetUsageRecordsAsync([FromQuery] GetChatAiUsageRecordsInput input)
    {
        return _aiUsageAppService.GetUsageRecordsAsync(input);
    }

    [HttpGet("policy")]
    [Authorize(ChatPermissions.AiUsage.View)]
    public virtual Task<ChatAiUsagePolicyDto> GetEffectivePolicyAsync([FromQuery] Guid? sessionId = null)
    {
        return _aiUsageAppService.GetEffectivePolicyAsync(sessionId);
    }
}
