using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

public static class ShortLinkGeneratorDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpShortLinkGenerator(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ShortUrl>(b =>
        {
            b.ToTable(SufiAbpShortLinkGeneratorDbProperties.DbTablePrefix + "ShortUrls",
                SufiAbpShortLinkGeneratorDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ShortCode)
                .IsRequired()
                .HasMaxLength(ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength);

            b.Property(x => x.DestinationUrl)
                .IsRequired()
                .HasMaxLength(ShortLinkGeneratorConsts.ShortUrl.MaxDestinationUrlLength);

            b.Property(x => x.Description)
                .HasMaxLength(ShortLinkGeneratorConsts.ShortUrl.MaxDescriptionLength);

            b.Property(x => x.CreatedByModule)
                .IsRequired()
                .HasMaxLength(ShortLinkGeneratorConsts.ShortUrl.MaxCreatedByModuleLength);

            b.HasIndex(x => x.ShortCode);
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.ExpiresAt);

        });

        builder.Entity<ShortUrlClick>(b =>
        {
            b.ToTable(SufiAbpShortLinkGeneratorDbProperties.DbTablePrefix + "ShortUrlClicks",
                SufiAbpShortLinkGeneratorDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.UserAgent)
                .HasMaxLength(500);

            b.Property(x => x.IpAddress)
                .HasMaxLength(50);

            b.Property(x => x.Referrer)
                .HasMaxLength(2048);

            b.Property(x => x.Token).HasMaxLength(256);
            b.Property(x => x.DedupKey).HasMaxLength(512);

            b.HasIndex(x => x.ShortUrlId);
            b.HasIndex(x => x.ClickedAt);
            b.HasIndex(x => new { x.ShortUrlId, x.DedupKey });
        });
    }
}
