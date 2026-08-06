using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Editions.MongoDB;

[ConnectionStringName(EditionsDbProperties.ConnectionStringName)]
public class EditionsMongoDbContext : AbpMongoDbContext, IEditionsMongoDbContext
{
    public IMongoCollection<Edition> Editions => Collection<Edition>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.ConfigureSufiEditions();
    }
}
