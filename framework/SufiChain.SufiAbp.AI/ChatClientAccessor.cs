using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IChatClientAccessor))]
public class ChatClientAccessor : IChatClientAccessor, ITransientDependency
{
    public IChatClient? ChatClient { get; }

    public ChatClientAccessor(IServiceProvider serviceProvider)
    {
        ChatClient = serviceProvider.GetKeyedService<IChatClient>(
            SufiAbpAIWorkspaceOptions.GetChatClientServiceKeyName(
                SufiAbpAIModule.DefaultWorkspaceName));
    }
}

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IChatClientAccessor<>))]
public class ChatClientAccessor<TWorkSpace> : IChatClientAccessor<TWorkSpace>, ITransientDependency
    where TWorkSpace : class
{
    public IChatClient? ChatClient { get; }

    public ChatClientAccessor(IServiceProvider serviceProvider)
    {
        ChatClient = serviceProvider.GetKeyedService<IChatClient>(
                SufiAbpAIWorkspaceOptions.GetChatClientServiceKeyName(
                    WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()))
                ??
            serviceProvider.GetRequiredKeyedService<IChatClient>(
                SufiAbpAIWorkspaceOptions.GetChatClientServiceKeyName(
                    SufiAbpAIModule.DefaultWorkspaceName))
        ;
    }
}
