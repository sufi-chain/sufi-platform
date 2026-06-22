using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIWorkspaceCatalog), typeof(ISufiAIWorkspaceResolver))]
public class SufiAIWorkspaceCatalogAdapter :
    ISufiAIWorkspaceCatalog,
    ISufiAIWorkspaceResolver,
    ITransientDependency
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    public SufiAIWorkspaceCatalogAdapter(IWorkspaceRepository workspaceRepository)
    {
        WorkspaceRepository = workspaceRepository;
    }

    public virtual async Task<List<SufiAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var workspaces = await WorkspaceRepository.GetListAsync(
            maxResultCount: int.MaxValue,
            sorting: nameof(Workspace.Name),
            cancellationToken: cancellationToken);

        return workspaces
            .Select(MapWorkspace)
            .OrderBy(workspace => workspace.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual async Task<SufiAIWorkspaceDescriptor?> FindAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var workspace = await WorkspaceRepository.FindByNameAsync(name.Trim(), cancellationToken);
        return workspace == null ? null : MapWorkspace(workspace);
    }

    public virtual async Task<SufiAIWorkspaceDescriptor?> ResolveAsync(
        string? preferredWorkspaceName = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(preferredWorkspaceName))
        {
            var preferred = await FindAsync(preferredWorkspaceName, cancellationToken);
            if (preferred is { IsActive: true, IsReady: true })
            {
                return preferred;
            }
        }

        return (await GetListAsync(cancellationToken)).FirstOrDefault(workspace => workspace.IsActive && workspace.IsReady);
    }

    protected virtual SufiAIWorkspaceDescriptor MapWorkspace(Workspace workspace)
    {
        return new SufiAIWorkspaceDescriptor
        {
            Name = workspace.Name,
            DisplayName = workspace.Name,
            Model = workspace.Model,
            IsActive = workspace.IsActive,
            IsReady = workspace.IsActive &&
                      !string.IsNullOrWhiteSpace(workspace.Model) &&
                      !string.IsNullOrWhiteSpace(workspace.ApiKey),
            Capabilities = MapCapabilities(workspace)
        };
    }

    protected virtual List<SufiAICapability> MapCapabilities(Workspace workspace)
    {
        var capabilities = new List<SufiAICapability>();

        if (!string.IsNullOrWhiteSpace(workspace.Model) || workspace.HasCapability(AICapabilityType.ChatCompletion))
        {
            capabilities.Add(SufiAICapability.Chat);
            capabilities.Add(SufiAICapability.Streaming);
        }

        if (workspace.HasCapability(AICapabilityType.AudioTranscription) ||
            workspace.HasCapability(AICapabilityType.TextToSpeech))
        {
            capabilities.Add(SufiAICapability.Audio);
        }

        if (workspace.HasCapability(AICapabilityType.VisionAnalysis))
        {
            capabilities.Add(SufiAICapability.Vision);
        }

        if (workspace.HasCapability(AICapabilityType.Embeddings) ||
            (!string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson) &&
             !string.IsNullOrWhiteSpace(workspace.VectorStoreConfigJson)))
        {
            capabilities.Add(SufiAICapability.Embeddings);
        }

        capabilities.Add(SufiAICapability.Tools);

        return capabilities.Distinct().ToList();
    }
}
