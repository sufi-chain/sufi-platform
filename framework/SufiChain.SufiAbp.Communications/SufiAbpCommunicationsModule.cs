using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.Communications.Email;
using SufiChain.SufiAbp.Communications.Localization;
using SufiChain.SufiAbp.Communications.Smtp;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Communications;

[DependsOn(
    typeof(AbpBackgroundJobsModule),
    typeof(AbpEventBusModule),
    typeof(AbpSettingsModule),
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule),
    typeof(SufiAbpTextTemplatingModule)
)]
public class SufiAbpCommunicationsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpCommunicationsModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            //if (!options.Resources.ContainsKey(typeof(CommunicationsResource)))
            //{
            //    options.Resources.Add<CommunicationsResource>("en");
            //}

            options.Resources.Get<CommunicationsResource>()
                .AddVirtualJson("/Localization/SufiAbpCommunications");
        });

        // Use NullEmailSender when SMTP is not configured; otherwise delegate to SmtpEmailSender.
        context.Services.Replace(ServiceDescriptor.Transient<IEmailSender, DynamicEmailSender>());

        // Background jobs are automatically registered via ITransientDependency
        // No manual registration needed - ABP discovers them automatically
    }
}
