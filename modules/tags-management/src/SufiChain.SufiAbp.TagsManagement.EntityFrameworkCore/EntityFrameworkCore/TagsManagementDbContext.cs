using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.TagsManagement.Tags;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

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

        builder.Entity<Tag>(b =>
        {
            b.ToTable(TagsManagementDbProperties.DbTablePrefix + "Tags", TagsManagementDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.Scope).IsRequired().HasMaxLength(TagScopeConsts.MaxScopeLength);
            b.Property(x => x.Color).HasMaxLength(TagConsts.MaxColorLength);
            b.HasIndex(x => new { x.TenantId, x.Scope, x.NormalizedName }).IsUnique();
        });

        builder.Entity<TagLink>(b =>
        {
            b.ToTable(TagsManagementDbProperties.DbTablePrefix + "TagLinks", TagsManagementDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(TagScopeConsts.MaxEntityTypeLength);
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.TagId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
            b.HasIndex(x => new { x.TenantId, x.TagId });
        });
    }
}
