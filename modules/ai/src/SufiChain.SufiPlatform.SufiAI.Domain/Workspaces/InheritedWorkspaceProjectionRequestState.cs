using System;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class InheritedWorkspaceProjectionRequestState : IScopedDependency
{
    public Guid? EnsuredTenantId { get; set; }
}
