using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;

[ConnectionStringName(SufiAuditLoggingDbProperties.ConnectionStringName)]
public class SufiAuditLoggingDbContext : AbpDbContext<SufiAuditLoggingDbContext>, IAuditLoggingDbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; set; }

    public SufiAuditLoggingDbContext(DbContextOptions<SufiAuditLoggingDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureAuditLogging();
    }
}
