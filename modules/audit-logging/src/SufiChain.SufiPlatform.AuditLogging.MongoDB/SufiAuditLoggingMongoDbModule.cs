using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.AuditLogging.MongoDB;

[DependsOn(typeof(SufiAuditLoggingDomainModule))]
[DependsOn(typeof(AbpMongoDbModule))]
public class SufiAuditLoggingMongoDbModule : AbpModule
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
