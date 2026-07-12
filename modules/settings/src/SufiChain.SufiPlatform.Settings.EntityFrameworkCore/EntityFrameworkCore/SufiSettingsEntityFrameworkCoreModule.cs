using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Settings.EntityFrameworkCore;

[DependsOn(
    typeof(SufiSettingsDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiSettingsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SettingsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            
            options.AddRepository<Setting, EfCoreSettingRepository>();
            options.AddRepository<SettingDefinitionRecord, EfCoreSettingDefinitionRecordRepository>();
        });
    }
}
