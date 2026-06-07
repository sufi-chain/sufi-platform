using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.TagsManagement.EntityFrameworkCore;

[ConnectionStringName(TagsManagementDbProperties.ConnectionStringName)]
public class TagsManagementDbContext : AbpDbContext<TagsManagementDbContext>, ITagsManagementDbContext
{
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TagLink> TagLinks { get; set; } = null!;

    public TagsManagementDbContext(DbContextOptions<TagsManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSufiAbpTagsManagement();
    }
}
