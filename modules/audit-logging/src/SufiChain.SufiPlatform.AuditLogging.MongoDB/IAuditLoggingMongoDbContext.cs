using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.AuditLogging.MongoDB;

[ConnectionStringName(SufiAuditLoggingDbProperties.ConnectionStringName)]
public interface IAuditLoggingMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<AuditLog> AuditLogs { get; }

    IMongoCollection<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
