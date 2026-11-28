using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Identity.EntityFrameworkCore;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Identity;

public class UnconfirmedAccountLookup : IUnconfirmedAccountLookup, ITransientDependency
{
    private readonly IDbContextProvider<ISufiIdentityDbContext> _dbContextProvider;
    private readonly ICurrentUser _currentUser;

    public UnconfirmedAccountLookup(
        IDbContextProvider<ISufiIdentityDbContext> dbContextProvider,
        ICurrentUser currentUser)
    {
        _dbContextProvider = dbContextProvider;
        _currentUser = currentUser;
    }

    public async Task<UnconfirmedAccountNotice> GetAsync()
    {
        var db = await _dbContextProvider.GetDbContextAsync();
        var count = await db.Users.CountAsync(user => !user.EmailConfirmed);
        var currentUnconfirmed = _currentUser.Id is Guid id
            && await db.Users.AnyAsync(user => user.Id == id && !user.EmailConfirmed);
        return new UnconfirmedAccountNotice(count, currentUnconfirmed);
    }
}
