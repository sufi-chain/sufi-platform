using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public interface IAuditLoggingDbContext : IEfCoreDbContext
{
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
