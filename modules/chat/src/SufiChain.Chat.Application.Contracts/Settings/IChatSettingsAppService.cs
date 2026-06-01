using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Settings;

public interface IChatSettingsAppService : IApplicationService
{
    Task<ChatSettingsDto> GetAsync();

    Task UpdateAsync(UpdateChatSettingsInput input);
}
