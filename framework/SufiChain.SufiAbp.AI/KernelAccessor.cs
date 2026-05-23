using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IKernelAccessor<>))]
public class KernelAccessor<TWorkSpace> : IKernelAccessor<TWorkSpace>, ITransientDependency
    where TWorkSpace : class
{
    public Kernel? Kernel { get; }

    public KernelAccessor(IServiceProvider serviceProvider)
    {
        Kernel = serviceProvider.GetKeyedService<Kernel>(
                SufiAbpAIWorkspaceOptions.GetKernelServiceKeyName(
                    WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()))
                ??
            serviceProvider.GetKeyedService<Kernel>(
                SufiAbpAIWorkspaceOptions.GetKernelServiceKeyName(
                    SufiAbpAIModule.DefaultWorkspaceName));
    }
}
