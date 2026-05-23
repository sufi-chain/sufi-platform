using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;

[ConnectionStringName(SufiAbpBlobStoringDatabaseDbProperties.ConnectionStringName)]
public class SufiAbpBlobStoringDbContext : AbpDbContext<SufiAbpBlobStoringDbContext>, ISufiAbpBlobStoringDbContext
{
    public DbSet<DatabaseBlobContainer> BlobContainers { get; set; } = null!;

    public DbSet<DatabaseBlob> Blobs { get; set; } = null!;

    public SufiAbpBlobStoringDbContext(DbContextOptions<SufiAbpBlobStoringDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiAbpBlobStoringDatabase();
    }
}
