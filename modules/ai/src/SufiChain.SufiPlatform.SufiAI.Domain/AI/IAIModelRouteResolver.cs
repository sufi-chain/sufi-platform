using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIModelRouteResolver : ITransientDependency
{
    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default);

    WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection);

    IReadOnlyList<WorkspaceRuntimeConfiguration> ListSelectableRoutes(
        Workspace workspace,
        AICapabilityType capabilityType,
        IReadOnlyCollection<Guid>? allowedModelConfigurationIds = null);
}
