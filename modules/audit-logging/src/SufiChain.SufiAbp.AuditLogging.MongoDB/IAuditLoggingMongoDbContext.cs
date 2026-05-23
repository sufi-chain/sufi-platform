using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public interface IAuditLoggingMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<AuditLog> AuditLogs { get; }

    IMongoCollection<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
