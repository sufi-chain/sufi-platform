using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

[ConnectionStringName(SufiAbpLocalizationManagementDbProperties.ConnectionStringName)]
public class LocalizationManagementDbContext : AbpDbContext<LocalizationManagementDbContext>, ISufiAbpLocalizationManagementDbContext
{
    public DbSet<LocalizationText> LocalizationTexts { get; set; } = null!;
    public DbSet<LocalizationResource> LocalizationResources { get; set; } = null!;

    public LocalizationManagementDbContext(DbContextOptions<LocalizationManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
