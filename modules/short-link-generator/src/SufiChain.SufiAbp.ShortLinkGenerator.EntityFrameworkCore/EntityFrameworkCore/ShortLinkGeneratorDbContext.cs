using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

[ConnectionStringName(SufiAbpShortLinkGeneratorDbProperties.ConnectionStringName)]
public class ShortLinkGeneratorDbContext : AbpDbContext<ShortLinkGeneratorDbContext>, ISufiAbpShortLinkGeneratorDbContext
{
    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<ShortUrlClick> ShortUrlClicks { get; set; }

    public ShortLinkGeneratorDbContext(DbContextOptions<ShortLinkGeneratorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}

