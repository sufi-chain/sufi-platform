using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAuditLoggingDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiAuditLoggingEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SufiAuditLoggingDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<AuditLog, EfCoreAuditLogRepository>();
        });
    }
}
