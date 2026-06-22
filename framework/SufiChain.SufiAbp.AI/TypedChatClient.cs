using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

public class TypedChatClient<TWorkSpace> : DelegatingChatClient, IChatClient<TWorkSpace>
    where TWorkSpace : class
{
    public TypedChatClient(IServiceProvider serviceProvider)
        : base(
            serviceProvider.GetKeyedService<IChatClient>(
                SufiAIWorkspaceOptions.GetChatClientServiceKeyName(
                    WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()))
                ??
            serviceProvider.GetRequiredKeyedService<IChatClient>(
                SufiAIWorkspaceOptions.GetChatClientServiceKeyName(
                    SufiAIModule.DefaultWorkspaceName))
        )
    {
    }
}
