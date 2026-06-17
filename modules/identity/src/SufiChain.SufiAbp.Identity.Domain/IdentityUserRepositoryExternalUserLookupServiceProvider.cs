using Microsoft.AspNetCore.Identity;
using SufiChain.SufiAbp.Users;
using Volo.Abp.DependencyInjection;
using SystemGuid = System.Guid;

namespace SufiChain.SufiAbp.Identity;

public class IdentityUserRepositoryExternalUserLookupServiceProvider : IExternalUserLookupServiceProvider, ITransientDependency
{
    protected IIdentityUserRepository UserRepository { get; }

    protected ILookupNormalizer LookupNormalizer { get; }

    public IdentityUserRepositoryExternalUserLookupServiceProvider(
        IIdentityUserRepository userRepository,
        ILookupNormalizer lookupNormalizer)
    {
        UserRepository = userRepository;
        LookupNormalizer = lookupNormalizer;
    }

    public virtual async Task<IUserData> FindByIdAsync(
        SystemGuid id,
        CancellationToken cancellationToken = default)
    {
        return (await UserRepository.FindAsync(
            id,
            includeDetails: false,
            cancellationToken: cancellationToken))?.ToAbpUserData();
    }

    public virtual async Task<IUserData> FindByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return (await UserRepository.FindByNormalizedUserNameAsync(
            LookupNormalizer.NormalizeName(userName),
            includeDetails: false,
            cancellationToken: cancellationToken))?.ToAbpUserData();
    }

    public virtual async Task<List<IUserData>> SearchAsync(
        string? sorting = null,
        string? filter = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var users = await UserRepository.GetListAsync(
            sorting: sorting,
            maxResultCount: maxResultCount,
            skipCount: skipCount,
            filter: filter,
            includeDetails: false,
            cancellationToken: cancellationToken);

        return users.Select(u => u.ToAbpUserData()).Cast<IUserData>().ToList();
    }

    public virtual async Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        return await UserRepository.GetCountAsync(filter, cancellationToken: cancellationToken);
    }
}
