using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IKernelAccessor))]
public class DefaultKernelAccessor : IKernelAccessor, ITransientDependency
{
    public Kernel? Kernel { get; }

    public DefaultKernelAccessor(IServiceProvider serviceProvider)
    {
        Kernel = serviceProvider.GetKeyedService<Kernel>(
            SufiAbpAIWorkspaceOptions.GetKernelServiceKeyName(
                SufiAbpAIModule.DefaultWorkspaceName));
    }
}
