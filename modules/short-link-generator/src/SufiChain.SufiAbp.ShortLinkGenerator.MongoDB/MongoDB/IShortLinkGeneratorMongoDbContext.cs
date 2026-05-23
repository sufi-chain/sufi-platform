using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;

[ConnectionStringName(SufiAbpShortLinkGeneratorDbProperties.ConnectionStringName)]
public interface IShortLinkGeneratorMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<ShortUrl> ShortUrls { get; }
    IMongoCollection<ShortUrlClick> ShortUrlClicks { get; }
}

