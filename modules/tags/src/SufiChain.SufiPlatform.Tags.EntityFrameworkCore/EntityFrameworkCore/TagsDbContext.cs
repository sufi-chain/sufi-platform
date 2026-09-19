using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public class TagsDbContext : AbpDbContext<TagsDbContext>, ITagsDbContext
{
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TagLink> TagLinks { get; set; } = null!;
    public DbSet<TagRelationDefinition> RelationDefinitions { get; set; } = null!;
    public DbSet<EntityRelation> Relations { get; set; } = null!;
    public DbSet<TagRelationMutationReceipt> RelationMutationReceipts { get; set; } = null!;

    public TagsDbContext(DbContextOptions<TagsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Register ownership before ABP applies entity conventions to discovered types.
        builder.Owned<EntityRelationRevision>();
        base.OnModelCreating(builder);

        builder.ConfigureSufiTags();
    }
}
