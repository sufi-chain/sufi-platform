using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Editions.MongoDB;

[ConnectionStringName(EditionsDbProperties.ConnectionStringName)]
public interface IEditionsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Edition> Editions { get; }
}
