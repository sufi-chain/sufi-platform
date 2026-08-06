using MongoDB.Driver;
using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Localization.MongoDB;

[ConnectionStringName(SufiLocalizationDbProperties.ConnectionStringName)]
public class LocalizationMongoDbContext : AbpMongoDbContext, ILocalizationMongoDbContext
{
    public IMongoCollection<LocalizationText> LocalizationTexts => Collection<LocalizationText>();
    public IMongoCollection<LocalizationResource> LocalizationResources => Collection<LocalizationResource>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.Entity<LocalizationText>(b =>
        {
            b.CollectionName = SufiLocalizationDbProperties.DbTablePrefix + "Texts";
        });

        modelBuilder.Entity<LocalizationResource>(b =>
        {
            b.CollectionName = SufiLocalizationDbProperties.DbTablePrefix + "Resources";
        });
    }
}
