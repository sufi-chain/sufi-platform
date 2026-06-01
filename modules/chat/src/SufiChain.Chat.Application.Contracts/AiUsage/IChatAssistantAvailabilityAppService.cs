using Volo.Abp.Application.Services;

namespace SufiChain.Chat.AiUsage;

public interface IChatAssistantAvailabilityAppService : IApplicationService
{
    Task<ChatAssistantAvailabilityDto> GetAsync();
}
