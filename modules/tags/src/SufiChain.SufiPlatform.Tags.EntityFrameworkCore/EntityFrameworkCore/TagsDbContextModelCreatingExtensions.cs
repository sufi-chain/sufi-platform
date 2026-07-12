using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

public static class TagsDbContextModelCreatingExtensions
{
    public static void ConfigureSufiTags(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Tag>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "Tags", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.Scope).IsRequired().HasMaxLength(TagScopeConsts.MaxScopeLength);
            b.Property(x => x.Color).HasMaxLength(TagConsts.MaxColorLength);
            b.HasIndex(x => new { x.TenantId, x.Scope, x.NormalizedName }).IsUnique();
        });

        builder.Entity<TagLink>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "TagLinks", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(TagScopeConsts.MaxEntityTypeLength);
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.TagId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
            b.HasIndex(x => new { x.TenantId, x.TagId });
        });
    }
}