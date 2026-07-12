using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAuditLoggingDbProperties.ConnectionStringName)]
public interface ISufiAuditLoggingDbContext : IEfCoreDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
