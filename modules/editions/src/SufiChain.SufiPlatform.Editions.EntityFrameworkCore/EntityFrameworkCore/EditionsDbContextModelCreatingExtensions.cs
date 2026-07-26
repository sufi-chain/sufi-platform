using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Editions.EntityFrameworkCore;

public static class EditionsDbContextModelCreatingExtensions
{
    public static void ConfigureSufiEditions(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Edition>(b =>
        {
            b.ToTable(EditionsDbProperties.DbTablePrefix + "Editions", EditionsDbProperties.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(EditionConsts.MaxNameLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(EditionConsts.MaxDisplayNameLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(EditionConsts.MaxCodeLength);
            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => x.Code).IsUnique();
        });
    }
}
