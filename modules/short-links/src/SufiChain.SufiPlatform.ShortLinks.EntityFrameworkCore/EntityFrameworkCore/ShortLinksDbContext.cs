using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;

[ConnectionStringName(SufiShortLinksDbProperties.ConnectionStringName)]
public class ShortLinksDbContext : AbpDbContext<ShortLinksDbContext>, ISufiShortLinksDbContext
{
    public DbSet<ShortUrl> ShortUrls { get; set; }
    public DbSet<ShortUrlClick> ShortUrlClicks { get; set; }

    public ShortLinksDbContext(DbContextOptions<ShortLinksDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}