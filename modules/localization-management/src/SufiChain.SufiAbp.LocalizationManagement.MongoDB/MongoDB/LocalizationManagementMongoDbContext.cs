using MongoDB.Driver;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.LocalizationManagement.MongoDB;

[ConnectionStringName(SufiAbpLocalizationManagementDbProperties.ConnectionStringName)]
public class LocalizationManagementMongoDbContext : AbpMongoDbContext, ILocalizationManagementMongoDbContext
{
    public IMongoCollection<LocalizationText> LocalizationTexts => Collection<LocalizationText>();
    public IMongoCollection<LocalizationResource> LocalizationResources => Collection<LocalizationResource>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.Entity<LocalizationText>(b =>
        {
            b.CollectionName = SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "Texts";
        });

        modelBuilder.Entity<LocalizationResource>(b =>
        {
            b.CollectionName = SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "Resources";
        });
    }
}
