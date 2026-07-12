using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

[ConnectionStringName(SufiTagsDbProperties.ConnectionStringName)]
public class TagsDbContext : AbpDbContext<TagsDbContext>, ITagsDbContext
{
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TagLink> TagLinks { get; set; } = null!;

    public TagsDbContext(DbContextOptions<TagsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiTags();
    }
}