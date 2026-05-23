using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Identity;

[Obsolete("Use IIdentityUserIntegrationService for module-to-module (or service-to-service) communication.")]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
public interface IIdentityUserLookupAppService : IApplicationService
{
    Task<UserData> FindByIdAsync(Guid id);

    Task<UserData> FindByUserNameAsync(string userName);

    Task<ListResultDto<UserData>> SearchAsync(UserLookupSearchInputDto input);

    Task<long> GetCountAsync(UserLookupCountInputDto input);
}
