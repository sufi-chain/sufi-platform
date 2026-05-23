using MongoDB.Driver;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.LocalizationManagement.MongoDB;

[ConnectionStringName(SufiAbpLocalizationManagementDbProperties.ConnectionStringName)]
public interface ILocalizationManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<LocalizationText> LocalizationTexts { get; }
    IMongoCollection<LocalizationResource> LocalizationResources { get; }
}
