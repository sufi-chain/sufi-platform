using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIWorkspaceCatalog), typeof(ISufiAIWorkspaceResolver))]
public class SufiAIWorkspaceCatalogAdapter :
    ISufiAIWorkspaceCatalog,
    ISufiAIWorkspaceResolver,
    ITransientDependency
{
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IWorkspaceRuntimeConfigurationResolver RuntimeConfigurationResolver { get; }

    public SufiAIWorkspaceCatalogAdapter(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver)
    {
        WorkspaceRepository = workspaceRepository;
        RuntimeConfigurationResolver = runtimeConfigurationResolver;
    }

    public virtual async Task<List<SufiAIWorkspaceDescriptor>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var workspaces = await WorkspaceRepository.GetListAsync(
            maxResultCount: int.MaxValue,
            sorting: nameof(Workspace.Name),
            cancellationToken: cancellationToken);

        var descriptors = new List<SufiAIWorkspaceDescriptor>();
        foreach (var workspace in workspaces)
        {
            descriptors.Add(MapWorkspace(workspace));
        }

        return descriptors
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
        return workspace == null
            ? null
            : MapWorkspace(workspace);
    }

    public virtual async Task<SufiAIWorkspaceDescriptor?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var workspace = await WorkspaceRepository.FindAsync(id, includeDetails: true, cancellationToken: cancellationToken);
        return workspace == null
            ? null
            : MapWorkspace(workspace);
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
        var results = new Dictionary<AICapabilityType, WorkspaceRuntimeConfiguration>();
        foreach (var capabilityType in Enum.GetValues<AICapabilityType>())
        {
            results[capabilityType] = RuntimeConfigurationResolver.Resolve(
                workspace,
                capabilityType);
        }

        var chat = results[AICapabilityType.ChatCompletion];
        return new SufiAIWorkspaceDescriptor
        {
            Id = workspace.Id,
            Name = workspace.Name,
            DisplayName = workspace.Name,
            Model = chat.ModelId,
            IsActive = workspace.IsActive,
            IsReady = chat.IsReady,
            Capabilities = MapCapabilities(results)
        };
    }

    protected virtual List<SufiAICapability> MapCapabilities(
        IReadOnlyDictionary<AICapabilityType, WorkspaceRuntimeConfiguration> results)
    {
        var capabilities = new List<SufiAICapability>();

        if (results[AICapabilityType.ChatCompletion].IsReady)
        {
            capabilities.Add(SufiAICapability.Chat);
            capabilities.Add(SufiAICapability.Streaming);
        }

        if (results[AICapabilityType.AudioTranscription].IsReady ||
            results[AICapabilityType.TextToSpeech].IsReady)
        {
            capabilities.Add(SufiAICapability.Audio);
        }

        if (results[AICapabilityType.VisionAnalysis].IsReady)
        {
            capabilities.Add(SufiAICapability.Vision);
        }

        if (results[AICapabilityType.Embeddings].IsReady)
        {
            capabilities.Add(SufiAICapability.Embeddings);
        }

        var chat = results[AICapabilityType.ChatCompletion];
        if (chat.IsReady &&
            chat.Provider == AIProviderType.OpenAI &&
            chat.OpenAIApiMode == OpenAIApiMode.ChatCompletions)
        {
            capabilities.Add(SufiAICapability.Tools);
        }

        return capabilities.Distinct().ToList();
    }
}
