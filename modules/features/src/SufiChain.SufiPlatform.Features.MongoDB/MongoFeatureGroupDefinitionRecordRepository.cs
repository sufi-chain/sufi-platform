using System;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Features.MongoDB;

public class MongoFeatureGroupDefinitionRecordRepository :
    MongoDbRepository<IFeaturesMongoDbContext, FeatureGroupDefinitionRecord, Guid>,
    IFeatureGroupDefinitionRecordRepository
{
    public MongoFeatureGroupDefinitionRecordRepository(
        IMongoDbContextProvider<IFeaturesMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
