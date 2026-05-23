using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;

[ConnectionStringName(SufiAbpBlobStoringDatabaseDbProperties.ConnectionStringName)]
public interface ISufiAbpBlobStoringDbContext : IEfCoreDbContext
{
    DbSet<DatabaseBlobContainer> BlobContainers { get; }

    DbSet<DatabaseBlob> Blobs { get; }
}
