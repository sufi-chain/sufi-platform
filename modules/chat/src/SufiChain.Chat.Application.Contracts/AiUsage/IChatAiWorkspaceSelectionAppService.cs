using Volo.Abp.Application.Services;

namespace SufiChain.Chat.AiUsage;

public interface IChatAiWorkspaceSelectionAppService : IApplicationService
{
    Task<ChatAiWorkspaceSelectionDto> GetAsync();

    Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync();

    Task UpdateDefaultAsync(UpdateChatAiWorkspaceSelectionInput input);
}
