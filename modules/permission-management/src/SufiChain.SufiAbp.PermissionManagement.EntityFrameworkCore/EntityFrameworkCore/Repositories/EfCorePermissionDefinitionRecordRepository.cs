using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

public class EfCorePermissionDefinitionRecordRepository :
    EfCoreRepository<ISufiAbpPermissionManagementDbContext, PermissionDefinitionRecord, Guid>,
    IPermissionDefinitionRecordRepository
{
    public EfCorePermissionDefinitionRecordRepository(
        IDbContextProvider<ISufiAbpPermissionManagementDbContext> dbContextProvider) 
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
