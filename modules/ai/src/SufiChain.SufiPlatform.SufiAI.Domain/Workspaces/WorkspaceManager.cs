using System;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceManager : DomainService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    
    public WorkspaceManager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }
    
    public async Task ValidateNameAsync(string name, Guid? excludeId = null)
    {
        var existing = await _workspaceRepository.FindByNameAsync(name);
        
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNameAlreadyExists)
                .WithData("Name", name);
        }
    }

    public async Task<string> ResolveUniqueNameAsync(string name, Guid? excludeId = null)
    {
        var desired = name.Trim();
        if (await _workspaceRepository.FindByNameAsync(desired) is not { } existing
            || existing.Id == excludeId)
        {
            return desired;
        }

        return $"{desired}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    public Workspace CreateCopy(
        Workspace source,
        string name,
        Guid? tenantId,
        Guid? id = null,
        bool includeCredentials = true)
    {
        // An inherited projection or its live view must never carry the host key into a new row.
        var keepSecrets = includeCredentials
            && !source.IsInherited
            && !InheritedWorkspaceLiveView.IsLiveView(source);
        var clone = new Workspace(
            id ?? GuidGenerator.Create(),
            name,
            source.Provider,
            source.DefaultModel,
            tenantId);

        clone.UpdateConfiguration(
            source.DefaultModel,
            keepSecrets ? source.ApiKey : null,
            source.ApiBaseUrl,
            source.InputCostPer1MTokens,
            source.OutputCostPer1MTokens);

        if (!source.IsActive)
        {
            clone.Deactivate();
        }

        foreach (var sourceConfiguration in source.ModelConfigurations)
        {
            clone.CopyModelConfiguration(sourceConfiguration, includeCredentials: keepSecrets);
        }

        foreach (var sourceGuardrail in source.Guardrails)
        {
            clone.SetGuardrail(sourceGuardrail.Period, sourceGuardrail.AmountUsd);
        }

        return clone;
    }
}
