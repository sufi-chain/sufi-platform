using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Editions.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Editions.Repositories;

public class EfCoreEditionRepository : EfCoreRepository<IEditionsDbContext, Edition, Guid>, IEditionRepository
{
    public EfCoreEditionRepository(IDbContextProvider<IEditionsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<Edition?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public virtual async Task<Edition?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
    }
}
