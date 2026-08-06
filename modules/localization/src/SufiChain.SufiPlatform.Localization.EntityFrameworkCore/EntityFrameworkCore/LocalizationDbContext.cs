using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Localization.EntityFrameworkCore;

[ConnectionStringName(SufiLocalizationDbProperties.ConnectionStringName)]
public class LocalizationDbContext : AbpDbContext<LocalizationDbContext>, ISufiLocalizationDbContext
{
    public DbSet<LocalizationText> LocalizationTexts { get; set; } = null!;
    public DbSet<LocalizationResource> LocalizationResources { get; set; } = null!;

    public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
