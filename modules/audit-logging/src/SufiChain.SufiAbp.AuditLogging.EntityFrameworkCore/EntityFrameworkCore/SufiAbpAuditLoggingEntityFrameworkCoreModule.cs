using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpAuditLoggingDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpAuditLoggingEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SufiAbpAuditLoggingDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<AuditLog, EfCoreAuditLogRepository>();
        });
    }
}
