using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Users;

public interface IExternalUserLookupServiceProvider
{
    Task<IUserData> FindByIdAsync(System.Guid id, CancellationToken cancellationToken = default);

    Task<IUserData> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<List<IUserData>> SearchAsync(
        string sorting = null,
        string filter = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string filter = null,
        CancellationToken cancellationToken = default
    );
}
