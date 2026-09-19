using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public interface ITagsDbContext : IEfCoreDbContext
{
    DbSet<Tag> Tags { get; }
    DbSet<TagLink> TagLinks { get; }
    DbSet<TagRelationDefinition> RelationDefinitions { get; }
    DbSet<EntityRelation> Relations { get; }
    DbSet<TagRelationMutationReceipt> RelationMutationReceipts { get; }
}
