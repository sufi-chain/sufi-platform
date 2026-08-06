using MongoDB.Driver;
using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Localization.MongoDB;

[ConnectionStringName(SufiLocalizationDbProperties.ConnectionStringName)]
public interface ILocalizationMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<LocalizationText> LocalizationTexts { get; }
    IMongoCollection<LocalizationResource> LocalizationResources { get; }
}
