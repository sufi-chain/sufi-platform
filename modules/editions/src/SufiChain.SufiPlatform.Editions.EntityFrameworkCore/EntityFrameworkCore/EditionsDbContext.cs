using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Editions.EntityFrameworkCore;

[ConnectionStringName(EditionsDbProperties.ConnectionStringName)]
public class EditionsDbContext : AbpDbContext<EditionsDbContext>, IEditionsDbContext
{
    public DbSet<Edition> Editions { get; set; } = null!;

    public EditionsDbContext(DbContextOptions<EditionsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiEditions();
    }
}
