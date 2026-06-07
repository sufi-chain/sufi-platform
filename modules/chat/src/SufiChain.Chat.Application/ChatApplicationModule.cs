using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Mapping;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatConnectorModule),
    typeof(ChatDomainModule),
    typeof(ChatApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpIdentityDomainModule),
    typeof(SufiAbpAIManagementDomainModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpSettingManagementDomainModule)
)]
public class ChatApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<IChatContactProvider, IdentityChatContactProvider>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatAiWorkspaceProvider, AIManagementChatAiWorkspaceProvider>());
        context.Services.Replace(ServiceDescriptor.Transient<IChatAssistantWorkspaceResolver, AIManagementChatAssistantWorkspaceResolver>());

        context.Services.AddMapperlyObjectMapper<ChatApplicationModule>();
        context.Services.AddTransient<ChatApplicationMapper>();
    }
}
