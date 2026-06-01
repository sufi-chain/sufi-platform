using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Connectors;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Settings;
using SufiChain.Chat.Transcripts;
using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class ChatHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers
                .Create(typeof(ChatApplicationContractsModule).Assembly, opts =>
                {
                    opts.RootPath = "chat";
                    opts.RemoteServiceName = ChatRemoteServiceConsts.RemoteServiceName;
                    opts.TypePredicate = type =>
                        type != typeof(IChatAiUsageAppService) &&
                        type != typeof(IChatAiWorkspaceSelectionAppService) &&
                        type != typeof(IChatAssistantAvailabilityAppService) &&
                        type != typeof(IChatContactAppService) &&
                        type != typeof(IConversationLinkAppService) &&
                        type != typeof(IChatMessageAppService) &&
                        type != typeof(IChatSessionAppService) &&
                        type != typeof(IChatTranscriptExporter) &&
                        type != typeof(IChatSettingsAppService) &&
                        type != typeof(IChatInboundMessageAppService);
                });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSignalR();
    }
}
