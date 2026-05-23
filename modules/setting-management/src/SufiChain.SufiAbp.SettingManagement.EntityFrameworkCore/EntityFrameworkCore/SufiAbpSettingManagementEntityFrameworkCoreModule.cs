using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpSettingManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SettingManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Setting, EfCoreSettingRepository>();
            options.AddRepository<SettingDefinitionRecord, EfCoreSettingDefinitionRecordRepository>();
        });
    }
}
