using System;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Permissions.MongoDB;

public class MongoPermissionGroupDefinitionRecordRepository :
    MongoDbRepository<IPermissionsMongoDbContext, PermissionGroupDefinitionRecord, Guid>,
    IPermissionGroupDefinitionRecordRepository
{
    public MongoPermissionGroupDefinitionRecordRepository(
        IMongoDbContextProvider<IPermissionsMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}