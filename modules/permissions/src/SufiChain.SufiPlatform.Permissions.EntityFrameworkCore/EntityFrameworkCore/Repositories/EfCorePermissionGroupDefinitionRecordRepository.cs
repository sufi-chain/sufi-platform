using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;

public class EfCorePermissionGroupDefinitionRecordRepository :
    EfCoreRepository<ISufiPermissionsDbContext, PermissionGroupDefinitionRecord, Guid>,
    IPermissionGroupDefinitionRecordRepository
{
    public EfCorePermissionGroupDefinitionRecordRepository(
        IDbContextProvider<ISufiPermissionsDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }
}