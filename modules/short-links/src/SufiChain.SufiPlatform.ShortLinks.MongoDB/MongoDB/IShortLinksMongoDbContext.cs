using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.ShortLinks.MongoDB.MongoDB;

[ConnectionStringName(SufiShortLinksDbProperties.ConnectionStringName)]
public interface IShortLinksMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<ShortUrl> ShortUrls { get; }
    IMongoCollection<ShortUrlClick> ShortUrlClicks { get; }
}