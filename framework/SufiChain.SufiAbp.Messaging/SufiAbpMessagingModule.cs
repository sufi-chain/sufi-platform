using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.Messaging.Email;
using SufiChain.SufiAbp.Messaging.Localization;
using SufiChain.SufiAbp.Messaging.Smtp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Messaging;

[DependsOn(
    typeof(AbpBackgroundJobsModule),
    typeof(AbpEventBusModule),
    typeof(AbpSettingsModule),
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
)]
public class SufiAbpMessagingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpMessagingModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<MessagingResource>("en")
                .AddVirtualJson("/Localization/SufiAbpMessaging");
        });

        // Use NullEmailSender when SMTP is not configured; otherwise delegate to SmtpEmailSender.
        context.Services.Replace(ServiceDescriptor.Transient<IEmailSender, DynamicEmailSender>());

        // Background jobs are automatically registered via ITransientDependency
        // No manual registration needed - ABP discovers them automatically
    }
}
