using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.FileManager.EntityFrameworkCore;

public static class FileManagerDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpFileManager(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<FileItem>(b =>
        {
            b.ToTable(SufiAbpFileManagerDbProperties.DbTablePrefix + "FileItems", SufiAbpFileManagerDbProperties.DbSchema);

            b.ConfigureByConvention();

            // Indexes
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.EntityType);
            b.HasIndex(x => x.EntityId);
            b.HasIndex(x => new { x.EntityType, x.EntityId });
            b.HasIndex(x => x.StructureKey);
            b.HasIndex(x => x.SourceEntityId);
            b.HasIndex(x => x.IsArchived);
            b.HasIndex(x => x.ArchivedAt);
            b.HasIndex(x => x.BlobName);
            b.HasIndex(x => x.FileType);
            b.HasIndex(x => x.IsTemp);
            b.HasIndex(x => x.CreationTime);

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.OriginalName).IsRequired().HasMaxLength(512);
            b.Property(x => x.BlobName).IsRequired().HasMaxLength(1024);
            b.Property(x => x.MimeType).IsRequired().HasMaxLength(128);
            b.Property(x => x.ThumbnailBlobName).HasMaxLength(1024);
            b.Property(x => x.EntityType).HasMaxLength(128);
            b.Property(x => x.Alt).HasMaxLength(512);
            b.Property(x => x.StructureKey).HasMaxLength(256);
            b.Property(x => x.CustomMetadata).HasMaxLength(4000);
            b.Property(x => x.ContentHash).HasMaxLength(64);

            // Configure Tags as JSON column
            b.Property(x => x.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : StringComparer.Ordinal.GetHashCode(item))),
                    value => value.ToList()));

            b.Property(x => x.Tags)
                .HasMaxLength(2048);

            // FolderId for custom folder reference
            b.HasIndex(x => x.FolderId);
        });

        builder.Entity<FileStructure>(b =>
        {
            b.ToTable(SufiAbpFileManagerDbProperties.DbTablePrefix + "FileStructures", SufiAbpFileManagerDbProperties.DbSchema);

            b.ConfigureByConvention();

            // Indexes
            b.HasIndex(x => x.Key).IsUnique();

            // Properties
            b.Property(x => x.Key).IsRequired().HasMaxLength(256);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(1024);
            b.Property(x => x.AllowedExtensions).IsRequired().HasMaxLength(512);
            b.Property(x => x.AllowedMimeTypes).IsRequired().HasMaxLength(1024);
            b.Property(x => x.StorageProvider).HasMaxLength(128);
            b.Property(x => x.BaseUrl).HasMaxLength(512);
        });

        builder.Entity<FileFolder>(b =>
        {
            b.ToTable(SufiAbpFileManagerDbProperties.DbTablePrefix + "FileFolders", SufiAbpFileManagerDbProperties.DbSchema);

            b.ConfigureByConvention();

            // Indexes
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.ParentId);
            b.HasIndex(x => new { x.TenantId, x.Path });
            b.HasIndex(x => x.Type);
            b.HasIndex(x => x.StructureKey);
            b.HasIndex(x => x.IsShared);

            // Properties
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Path).IsRequired().HasMaxLength(1024);
            b.Property(x => x.StructureKey).HasMaxLength(256);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.Color).HasMaxLength(32);
            b.Property(x => x.SharedWithTenants).HasMaxLength(4000);
            b.Property(x => x.Description).HasMaxLength(1024);

            // Self-referencing relationship for parent/child
            b.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship with permissions
            b.HasMany(x => x.Permissions)
                .WithOne(x => x.Folder)
                .HasForeignKey(x => x.FolderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FolderPermission>(b =>
        {
            b.ToTable(SufiAbpFileManagerDbProperties.DbTablePrefix + "FolderPermissions", SufiAbpFileManagerDbProperties.DbSchema);

            b.ConfigureByConvention();

            // Match FileFolder's global filters so permissions for filtered folders are filtered too.
            b.HasQueryFilter(x => x.Folder != null);

            // Indexes
            b.HasIndex(x => x.FolderId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.RoleId);
            b.HasIndex(x => new { x.FolderId, x.UserId });
            b.HasIndex(x => new { x.FolderId, x.RoleId });
        });
    }
}
