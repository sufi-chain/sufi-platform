using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

public class EfCorePermissionGroupDefinitionRecordRepository :
    EfCoreRepository<ISufiAbpPermissionManagementDbContext, PermissionGroupDefinitionRecord, Guid>,
    IPermissionGroupDefinitionRecordRepository
{
    public EfCorePermissionGroupDefinitionRecordRepository(
        IDbContextProvider<ISufiAbpPermissionManagementDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }
}