using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;

[ConnectionStringName(SufiAbpShortLinkGeneratorDbProperties.ConnectionStringName)]
public class ShortLinkGeneratorMongoDbContext : AbpMongoDbContext, IShortLinkGeneratorMongoDbContext
{
    public IMongoCollection<ShortUrl> ShortUrls => Collection<ShortUrl>();
    public IMongoCollection<ShortUrlClick> ShortUrlClicks => Collection<ShortUrlClick>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

    }
}

