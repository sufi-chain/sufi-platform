using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

[DependsOn(typeof(SufiAbpAuditLoggingDomainModule))]
[DependsOn(typeof(AbpMongoDbModule))]
public class SufiAbpAuditLoggingMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<AuditLoggingMongoDbContext>(options =>
        {
            options.AddRepository<AuditLog, MongoAuditLogRepository>();
            options.AddRepository<AuditLogExcelFile, MongoAuditLogExcelFileRepository>();
        });
    }
}
