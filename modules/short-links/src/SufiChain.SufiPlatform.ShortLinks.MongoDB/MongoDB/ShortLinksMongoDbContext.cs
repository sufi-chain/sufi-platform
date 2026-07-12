using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.ShortLinks.MongoDB.MongoDB;

[ConnectionStringName(SufiShortLinksDbProperties.ConnectionStringName)]
public class ShortLinksMongoDbContext : AbpMongoDbContext, IShortLinksMongoDbContext
{
    public IMongoCollection<ShortUrl> ShortUrls => Collection<ShortUrl>();
    public IMongoCollection<ShortUrlClick> ShortUrlClicks => Collection<ShortUrlClick>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

    }
}