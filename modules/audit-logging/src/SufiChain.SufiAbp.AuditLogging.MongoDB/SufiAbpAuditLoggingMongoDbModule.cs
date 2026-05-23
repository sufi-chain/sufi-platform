using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.AuditLogging.MongoDB;

[DependsOn(typeof(SufiAbpAuditLoggingDomainModule))]
[DependsOn(typeof(SufiAbpMongoDbModule))]
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
