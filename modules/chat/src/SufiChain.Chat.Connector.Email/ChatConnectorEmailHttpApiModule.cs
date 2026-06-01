using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(ChatConnectorEmailModule),
    typeof(SufiChain.SufiAbp.AspNetCore.Mvc.SufiAbpAspNetCoreMvcModule)
)]
public class ChatConnectorEmailHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(ChatConnectorEmailModule).Assembly, opts =>
            {
                opts.RootPath = "chat";
                opts.RemoteServiceName = ChatRemoteServiceConsts.RemoteServiceName;
            });
        });
    }
}
