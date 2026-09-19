using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

[DependsOn(
    typeof(SufiTagsDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiTagsMongoDbModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        BsonClassMap.TryRegisterClassMap<EntityRelation>(map =>
        {
            map.AutoMap();
            map.UnmapMember(x => x.Revisions);
            map.MapField("_revisions").SetElementName("Revisions");
        });
        BsonClassMap.TryRegisterClassMap<TagRelationMutationReceipt>(map =>
        {
            map.AutoMap();
            map.MapMember(x => x.RecordedAtUtc).SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<TagsMongoDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Tags.Tag, Repositories.MongoTagRepository>();
            options.AddRepository<Tags.TagLink, Repositories.MongoTagLinkRepository>();
            options.AddRepository<EntityRelation, Repositories.MongoEntityRelationRepository>();
        });
    }
}
