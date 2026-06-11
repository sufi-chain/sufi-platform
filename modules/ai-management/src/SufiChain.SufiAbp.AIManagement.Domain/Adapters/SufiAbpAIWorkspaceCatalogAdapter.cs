using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIWorkspaceCatalog), typeof(ISufiAbpAIWorkspaceResolver))]
public class SufiAbpAIWorkspaceCatalogAdapter :
    ISufiAbpAIWorkspaceCatalog,
    ISufiAbpAIWorkspaceResolver,
    ITransientDependency
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    public SufiAbpAIWorkspaceCatalogAdapter(IWorkspaceRepository workspaceRepository)
    {
        WorkspaceRepository = workspaceRepository;
    }

    public virtual async Task<List<SufiAbpAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
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

    public virtual async Task<SufiAbpAIWorkspaceDescriptor?> FindAsync(
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

    public virtual async Task<SufiAbpAIWorkspaceDescriptor?> ResolveAsync(
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

    protected virtual SufiAbpAIWorkspaceDescriptor MapWorkspace(Workspace workspace)
    {
        return new SufiAbpAIWorkspaceDescriptor
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

    protected virtual List<SufiAbpAICapability> MapCapabilities(Workspace workspace)
    {
        var capabilities = new List<SufiAbpAICapability>();

        if (!string.IsNullOrWhiteSpace(workspace.Model) || workspace.HasCapability(AICapabilityType.ChatCompletion))
        {
            capabilities.Add(SufiAbpAICapability.Chat);
            capabilities.Add(SufiAbpAICapability.Streaming);
        }

        if (workspace.HasCapability(AICapabilityType.AudioTranscription) ||
            workspace.HasCapability(AICapabilityType.TextToSpeech))
        {
            capabilities.Add(SufiAbpAICapability.Audio);
        }

        if (workspace.HasCapability(AICapabilityType.VisionAnalysis))
        {
            capabilities.Add(SufiAbpAICapability.Vision);
        }

        if (workspace.HasCapability(AICapabilityType.Embeddings) ||
            (!string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson) &&
             !string.IsNullOrWhiteSpace(workspace.VectorStoreConfigJson)))
        {
            capabilities.Add(SufiAbpAICapability.Embeddings);
        }

        capabilities.Add(SufiAbpAICapability.Tools);

        return capabilities.Distinct().ToList();
    }
}
