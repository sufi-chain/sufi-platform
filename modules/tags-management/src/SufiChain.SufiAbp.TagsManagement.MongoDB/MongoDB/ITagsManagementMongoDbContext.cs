using MongoDB.Driver;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.TagsManagement.MongoDB;

[ConnectionStringName(TagsManagementDbProperties.ConnectionStringName)]
public interface ITagsManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Tags.Tag> Tags { get; }
    IMongoCollection<TagLink> TagLinks { get; }
}
