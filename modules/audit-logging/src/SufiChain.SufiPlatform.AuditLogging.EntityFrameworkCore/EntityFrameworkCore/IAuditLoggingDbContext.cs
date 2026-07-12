using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAuditLoggingDbProperties.ConnectionStringName)]
public interface IAuditLoggingDbContext : IEfCoreDbContext
{
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
