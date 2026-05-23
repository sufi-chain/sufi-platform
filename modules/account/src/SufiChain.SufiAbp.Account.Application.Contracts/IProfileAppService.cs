using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Account;

public interface IProfileAppService : IApplicationService
{
    Task<ProfileDto> GetAsync();

    Task<ProfileDto> UpdateAsync(UpdateProfileDto input);

    Task ChangePasswordAsync(ChangePasswordInput input);
}
