using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;

public class EfCorePermissionDefinitionRecordRepository :
    EfCoreRepository<ISufiPermissionsDbContext, PermissionDefinitionRecord, Guid>,
    IPermissionDefinitionRecordRepository
{
    public EfCorePermissionDefinitionRecordRepository(
        IDbContextProvider<ISufiPermissionsDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public virtual async Task<PermissionDefinitionRecord> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }
}
