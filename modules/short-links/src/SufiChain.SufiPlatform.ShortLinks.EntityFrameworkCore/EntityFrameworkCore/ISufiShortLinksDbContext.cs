using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;

[ConnectionStringName(SufiShortLinksDbProperties.ConnectionStringName)]
public interface ISufiShortLinksDbContext : IEfCoreDbContext
{
    DbSet<ShortUrl> ShortUrls { get; }
    DbSet<ShortUrlClick> ShortUrlClicks { get; }
}