using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;

public class EfCoreDatabaseBlobRepository : EfCoreRepository<ISufiBlobDatabaseDbContext, DatabaseBlob, Guid>, IDatabaseBlobRepository
{
    public EfCoreDatabaseBlobRepository(IDbContextProvider<ISufiBlobDatabaseDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<DatabaseBlob?> FindAsync(Guid containerId, string name, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .FirstOrDefaultAsync(
                x => x.ContainerId == containerId && x.Name == name,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> ExistsAsync(Guid containerId, string name, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .AnyAsync(
                x => x.ContainerId == containerId && x.Name == name,
                GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> DeleteAsync(Guid containerId, string name, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var blob = await FindAsync(containerId, name, cancellationToken);
        if (blob == null)
        {
            return false;
        }

        await base.DeleteAsync(blob, autoSave, cancellationToken: GetCancellationToken(cancellationToken));
        return true;
    }
}
