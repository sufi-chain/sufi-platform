using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Tags.EntityFrameworkCore;

public static class TagsDbContextModelCreatingExtensions
{
    public static void ConfigureSufiTags(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<TagRelationDefinition>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "RelationDefinitions", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TenantScopeKey).IsRequired().HasMaxLength(32);
            b.Property(x => x.StableKey).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.Property(x => x.SourceEntityType).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.Property(x => x.TargetEntityType).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.HasIndex(x => new { x.TenantScopeKey, x.StableKey, x.Revision }).IsUnique();
            b.HasIndex(x => new { x.TenantScopeKey, x.PredicateTagId, x.Revision }).IsUnique();
        });

        builder.Entity<EntityRelation>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "Relations", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TenantScopeKey).IsRequired().HasMaxLength(32);
            b.Property(x => x.ScopeType).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.Property(x => x.SourceType).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.Property(x => x.TargetType).IsRequired().HasMaxLength(TagEntityReference.MaxEntityTypeLength);
            b.HasIndex(x => new { x.TenantScopeKey, x.ScopeType, x.ScopeId, x.PredicateTagId,
                x.SourceType, x.SourceId, x.TargetType, x.TargetId }).IsUnique();
            b.OwnsMany(x => x.Revisions, revision =>
            {
                revision.ToTable(SufiTagsDbProperties.DbTablePrefix + "RelationRevisions", SufiTagsDbProperties.DbSchema);
                revision.WithOwner().HasForeignKey("RelationId");
                revision.HasKey("RelationId", nameof(EntityRelationRevision.Sequence));
                revision.Property(x => x.Sequence).ValueGeneratedNever();
                revision.Property(x => x.Reason).IsRequired().HasMaxLength(EntityRelation.MaxReasonLength);
                revision.Property(x => x.RecordedAtUtc).HasConversion(
                    value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
                revision.Property(x => x.ValidFromUtc).HasConversion(
                    value => value, value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
                revision.Property(x => x.ValidToUtc).HasConversion(
                    value => value, value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
                revision.HasIndex("RelationId", nameof(EntityRelationRevision.RequestId)).IsUnique();
            });
            b.Navigation(x => x.Revisions).HasField("_revisions").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<TagRelationMutationReceipt>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "RelationMutationReceipts", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.TenantScopeKey).IsRequired().HasMaxLength(32);
            b.Property(x => x.RecordedAtUtc).HasConversion(
                value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            b.HasIndex(x => new { x.TenantScopeKey, x.RelationId, x.AppliedVersion });
        });

        builder.Entity<Tag>(b =>
        {
            b.ToTable(SufiTagsDbProperties.DbTablePrefix + "Tags", SufiTagsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(TagConsts.MaxNameLength);
            b.Property(x => x.Scope).IsRequired().HasMaxLength(TagScopeConsts.MaxScopeLength);
            b.Property(x => x.Color).HasMaxLength(TagConsts.MaxColorLength);
            b.Property(x => x.Kind).HasDefaultValue(TagKind.Classification);
            b.Property(x => x.StableKey).HasMaxLength(TagEntityReference.MaxEntityTypeLength);
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
