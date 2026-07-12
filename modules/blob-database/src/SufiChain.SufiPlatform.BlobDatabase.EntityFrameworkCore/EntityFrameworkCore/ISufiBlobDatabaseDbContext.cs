using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;

[ConnectionStringName(SufiBlobDatabaseDbProperties.ConnectionStringName)]
public interface ISufiBlobDatabaseDbContext : IEfCoreDbContext
{
    DbSet<DatabaseBlobContainer> BlobContainers { get; }

    DbSet<DatabaseBlob> Blobs { get; }
}
