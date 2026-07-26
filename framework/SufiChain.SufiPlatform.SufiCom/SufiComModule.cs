using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Email;
using SufiChain.SufiPlatform.SufiCom.Localization;
using SufiChain.SufiPlatform.SufiCom.Smtp;
using SufiChain.SufiPlatform.TextTemplating;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiPlatform.SufiCom;

[DependsOn(
    typeof(AbpBackgroundJobsModule),
    typeof(AbpEventBusModule),
    typeof(AbpSettingsModule),
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule),
    typeof(SufiTextTemplatingModule)
)]
public class SufiComModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiComModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            // Distinct from Domain.Shared path /Localization/SufiCom — same RootNamespace would
            // otherwise overwrite module en.json and break menu keys (Menu:Communication, etc.).
            if (!options.Resources.ContainsResource(typeof(SufiComResource)))
            {
                options.Resources.Add<SufiComResource>("en");
            }

            options.Resources.Get<SufiComResource>()
                .AddVirtualJson("/Localization/SufiComFramework");
        });

        // Use NullEmailSender when SMTP is not configured; otherwise delegate to SmtpEmailSender.
        context.Services.Replace(ServiceDescriptor.Transient<IEmailSender, DynamicEmailSender>());

        // Background jobs are automatically registered via ITransientDependency
        // No manual registration needed - ABP discovers them automatically
    }
}
