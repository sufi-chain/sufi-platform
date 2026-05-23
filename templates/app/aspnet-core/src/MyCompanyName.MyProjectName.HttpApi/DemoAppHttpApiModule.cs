using MyCompanyName.MyProjectName.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName
{
    [DependsOn(
        typeof(DemoAppApplicationContractsModule)
        )]
    public class DemoAppHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureLocalization();
        }

        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Get<DemoAppResource>();
            });
        }
    }
}
