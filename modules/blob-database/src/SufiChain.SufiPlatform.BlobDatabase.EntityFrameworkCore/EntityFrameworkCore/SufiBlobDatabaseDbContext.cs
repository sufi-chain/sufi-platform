using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;

[ConnectionStringName(SufiBlobDatabaseDbProperties.ConnectionStringName)]
public class SufiBlobDatabaseDbContext : AbpDbContext<SufiBlobDatabaseDbContext>, ISufiBlobDatabaseDbContext
{
    public DbSet<DatabaseBlobContainer> BlobContainers { get; set; } = null!;

    public DbSet<DatabaseBlob> Blobs { get; set; } = null!;

    public SufiBlobDatabaseDbContext(DbContextOptions<SufiBlobDatabaseDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiBlobDatabase();
    }
}
