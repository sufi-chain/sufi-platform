using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;

public static class SufiAbpBlobStoringDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpBlobStoringDatabase(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<DatabaseBlobContainer>(b =>
        {
            b.ToTable(
                SufiAbpBlobStoringDatabaseDbProperties.DbTablePrefix + "BlobContainers",
                SufiAbpBlobStoringDatabaseDbProperties.DbSchema);

            b.ConfigureByConvention();
            b.Property(p => p.Name).IsRequired().HasMaxLength(DatabaseContainerConsts.MaxNameLength);
            b.HasMany<DatabaseBlob>().WithOne().HasForeignKey(p => p.ContainerId);
            b.HasIndex(x => new { x.TenantId, x.Name });
            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<DatabaseBlob>(b =>
        {
            b.ToTable(
                SufiAbpBlobStoringDatabaseDbProperties.DbTablePrefix + "Blobs",
                SufiAbpBlobStoringDatabaseDbProperties.DbSchema);

            b.ConfigureByConvention();
            b.Property(p => p.ContainerId).IsRequired();
            b.Property(p => p.Name).IsRequired().HasMaxLength(DatabaseBlobConsts.MaxNameLength);
            b.Property(p => p.Content).HasMaxLength(DatabaseBlobConsts.MaxContentLength);
            b.HasOne<DatabaseBlobContainer>().WithMany().HasForeignKey(p => p.ContainerId);
            b.HasIndex(x => new { x.TenantId, x.ContainerId, x.Name });
            b.ApplyObjectExtensionMappings();
        });

        builder.TryConfigureObjectExtensions<SufiAbpBlobStoringDbContext>();
    }
}
