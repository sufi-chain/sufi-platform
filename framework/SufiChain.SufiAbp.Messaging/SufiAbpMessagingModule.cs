using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.Messaging.Email;
using SufiChain.SufiAbp.Messaging.Localization;
using SufiChain.SufiAbp.Messaging.Smtp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Messaging;

[DependsOn(
    typeof(AbpBackgroundJobsModule),
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

        // Register SMTP as default email sender
        context.Services.Replace(ServiceDescriptor.Transient<IEmailSender, SmtpEmailSender>());
        context.Services.Replace(ServiceDescriptor.Transient<IEmailSenderConfiguration, SmtpEmailSenderConfiguration>());
        
        // Background jobs are automatically registered via ITransientDependency
        // No manual registration needed - ABP discovers them automatically
    }
}
