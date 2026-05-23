using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

public static class LocalizationManagementDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpLocalizationManagement(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<LocalizationResource>(b =>
        {
            b.ToTable(SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "LocalizationResources", SufiAbpLocalizationManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.ResourceName);
            b.HasIndex(x => x.IsEnabled);

            b.Property(x => x.ResourceName).IsRequired().HasMaxLength(128);
            b.Property(x => x.DefaultCulture).IsRequired().HasMaxLength(16);
            b.Property(x => x.DisplayName).HasMaxLength(256);

            b.Property(x => x.BaseResourceNames)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : StringComparer.Ordinal.GetHashCode(item))),
                    value => value.ToList()));

            b.Property(x => x.BaseResourceNames)
                .HasMaxLength(2048);
        });

        builder.Entity<LocalizationText>(b =>
        {
            b.ToTable(SufiAbpLocalizationManagementDbProperties.DbTablePrefix + "LocalizationTexts", SufiAbpLocalizationManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => x.ResourceName);
            b.HasIndex(x => x.CultureName);
            b.HasIndex(x => new { x.ResourceName, x.CultureName, x.Key }).IsUnique();

            b.Property(x => x.ResourceName).IsRequired().HasMaxLength(128);
            b.Property(x => x.CultureName).IsRequired().HasMaxLength(16);
            b.Property(x => x.Key).IsRequired().HasMaxLength(512);
            b.Property(x => x.Value).IsRequired().HasMaxLength(4096);
        });
    }
}
