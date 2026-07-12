using MongoDB.Driver;
using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.AuditLogging.MongoDB;

[ConnectionStringName(SufiAuditLoggingDbProperties.ConnectionStringName)]
public interface ISufiAuditLoggingMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<AuditLog> AuditLogs { get; }
    IMongoCollection<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
