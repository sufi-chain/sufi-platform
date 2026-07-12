using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IKernelAccessor<>))]
public class KernelAccessor<TWorkSpace> : IKernelAccessor<TWorkSpace>, ITransientDependency
    where TWorkSpace : class
{
    public Kernel? Kernel { get; }

    public KernelAccessor(IServiceProvider serviceProvider)
    {
        Kernel = serviceProvider.GetKeyedService<Kernel>(
                SufiAIWorkspaceOptions.GetKernelServiceKeyName(
                    WorkspaceNameAttribute.GetWorkspaceName<TWorkSpace>()))
                ??
            serviceProvider.GetKeyedService<Kernel>(
                SufiAIWorkspaceOptions.GetKernelServiceKeyName(
                    SufiAIModule.DefaultWorkspaceName));
    }
}
