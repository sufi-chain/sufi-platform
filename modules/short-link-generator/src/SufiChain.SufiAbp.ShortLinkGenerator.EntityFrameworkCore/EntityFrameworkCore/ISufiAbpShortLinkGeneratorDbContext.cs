using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

[ConnectionStringName(SufiAbpShortLinkGeneratorDbProperties.ConnectionStringName)]
public interface ISufiAbpShortLinkGeneratorDbContext : IEfCoreDbContext
{
    DbSet<ShortUrl> ShortUrls { get; }
    DbSet<ShortUrlClick> ShortUrlClicks { get; }
}

