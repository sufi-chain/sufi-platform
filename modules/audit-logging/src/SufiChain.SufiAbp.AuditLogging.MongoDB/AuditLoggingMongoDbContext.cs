using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

[ConnectionStringName(SufiAbpAuditLoggingDbProperties.ConnectionStringName)]
public class AuditLoggingMongoDbContext : AbpMongoDbContext, IAuditLoggingMongoDbContext
{
    public IMongoCollection<AuditLog> AuditLogs => Collection<AuditLog>();

    public IMongoCollection<AuditLogExcelFile> AuditLogExcelFiles => Collection<AuditLogExcelFile>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureAuditLogging();
    }
}
