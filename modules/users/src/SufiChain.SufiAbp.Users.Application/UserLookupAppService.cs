using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Users.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.Users;

[Authorize(UsersPermissions.UserLookup.Default)]
public class UserLookupAppService : SufiAbpApplicationService, IUserLookupAppService
{
    protected IExternalUserLookupServiceProvider ExternalUserLookupServiceProvider { get; }

    public UserLookupAppService(IExternalUserLookupServiceProvider externalUserLookupServiceProvider)
    {
        ExternalUserLookupServiceProvider = externalUserLookupServiceProvider;
    }

    public virtual async Task<UserLookupDto> GetAsync(Guid id)
    {
        var user = await ExternalUserLookupServiceProvider.FindByIdAsync(id);
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IUserData), id);
        }

        return MapToDto(user);
    }

    public virtual async Task<PagedResultDto<UserLookupDto>> SearchAsync(UserLookupSearchInput input)
    {
        var count = await ExternalUserLookupServiceProvider.GetCountAsync(input.Filter);
        var users = await ExternalUserLookupServiceProvider.SearchAsync(
            input.Sorting,
            input.Filter,
            input.MaxResultCount,
            input.SkipCount);

        return new PagedResultDto<UserLookupDto>(
            count,
            users.Select(MapToDto).ToList());
    }

    public virtual Task<long> GetCountAsync(UserLookupCountInput input)
    {
        return ExternalUserLookupServiceProvider.GetCountAsync(input.Filter);
    }

    protected virtual UserLookupDto MapToDto(IUserData user)
    {
        return new UserLookupDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };
    }
}
