using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.Mapping;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatConnectorModule),
    typeof(ChatDomainModule),
    typeof(ChatApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpSettingManagementDomainModule)
)]
public class ChatApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<ChatApplicationModule>();
        context.Services.AddTransient<ChatApplicationMapper>();
    }
}
