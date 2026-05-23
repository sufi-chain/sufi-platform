using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public interface ISufiAbpAuditLoggingDbContext : IEfCoreDbContext
{
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; }
}
