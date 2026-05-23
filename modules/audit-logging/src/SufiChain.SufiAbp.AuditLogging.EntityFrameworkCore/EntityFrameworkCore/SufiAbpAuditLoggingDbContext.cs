using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public class SufiAbpAuditLoggingDbContext : AbpDbContext<SufiAbpAuditLoggingDbContext>, IAuditLoggingDbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; set; }

    public SufiAbpAuditLoggingDbContext(DbContextOptions<SufiAbpAuditLoggingDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureAuditLogging();
    }
}
