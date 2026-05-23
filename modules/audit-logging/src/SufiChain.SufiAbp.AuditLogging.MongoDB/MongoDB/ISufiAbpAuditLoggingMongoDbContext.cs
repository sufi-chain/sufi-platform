using MongoDB.Driver;
using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public interface ISufiAbpAuditLoggingMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<AuditLog> AuditLogs { get; }
    IMongoCollection<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
