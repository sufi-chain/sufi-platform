using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.FileManager.Repositories;

public class EfCoreFileStructureRepository :
    EfCoreRepository<ISufiAbpFileManagerDbContext, FileStructure, Guid>,
    IFileStructureRepository
{
    public EfCoreFileStructureRepository(IDbContextProvider<ISufiAbpFileManagerDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<FileStructure?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<bool> KeyExistsAsync(
        string key,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.Key == key);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
