using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;

public class EfCoreDatabaseBlobContainerRepository : EfCoreRepository<ISufiAbpBlobStoringDbContext, DatabaseBlobContainer, Guid>, IDatabaseBlobContainerRepository
{
    public EfCoreDatabaseBlobContainerRepository(IDbContextProvider<ISufiAbpBlobStoringDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<DatabaseBlobContainer?> FindAsync(string name, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .FirstOrDefaultAsync(x => x.Name == name, GetCancellationToken(cancellationToken));
    }
}
