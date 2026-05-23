using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Emailing.Localization;
using Volo.Abp.Localization;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Emailing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Emailing;

[DependsOn(
    typeof(AbpEmailingModule)
)]
public class SufiAbpEmailingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpEmailingModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SufiAbpEmailingResource>("en")
                .AddVirtualJson("/Localization/SufiAbpEmailing");
        });

        context.Services.AddTransient<SufiChain.SufiAbp.Emailing.IEmailSender, SufiAbpEmailSender>();
    }
}
