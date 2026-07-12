using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Tags.MongoDB;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public interface ITagsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tags.Tag> Tags { get; }
    IMongoCollection<TagLink> TagLinks { get; }
}