using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Users;

[RemoteService(Name = UsersRemoteServiceConsts.RemoteServiceName)]
public interface IUserLookupAppService : IApplicationService
{
    Task<UserLookupDto> GetAsync(Guid id);

    Task<PagedResultDto<UserLookupDto>> SearchAsync(UserLookupSearchInput input);

    Task<long> GetCountAsync(UserLookupCountInput input);
}
