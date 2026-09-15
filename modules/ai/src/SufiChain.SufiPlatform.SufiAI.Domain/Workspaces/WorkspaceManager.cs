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

    public Workspace CreateCopy(Workspace source, string name, Guid? tenantId, Guid? id = null)
    {
        var clone = new Workspace(
            id ?? GuidGenerator.Create(),
            name,
            source.Provider,
            source.DefaultModel,
            tenantId);

        clone.UpdateConfiguration(
            source.DefaultModel,
            source.ApiKey,
            source.ApiBaseUrl,
            source.InputCostPer1MTokens,
            source.OutputCostPer1MTokens);

        if (!source.IsActive)
        {
            clone.Deactivate();
        }

        foreach (var sourceConfiguration in source.ModelConfigurations)
        {
            var clonedConfiguration = clone.AddModelConfiguration(
                sourceConfiguration.CapabilityType,
                sourceConfiguration.ModelId,
                sourceConfiguration.ApiEndpoint,
                sourceConfiguration.ApiKey,
                sourceConfiguration.Priority,
                sourceConfiguration.OpenAIApiMode,
                sourceConfiguration.InputCostPer1MTokens,
                sourceConfiguration.OutputCostPer1MTokens,
                sourceConfiguration.Dimensions,
                sourceConfiguration.DisplayName,
                sourceConfiguration.IsUserSelectable,
                sourceConfiguration.Description,
                sourceConfiguration.MaxContextTokens);

            if (!sourceConfiguration.IsEnabled)
            {
                clonedConfiguration.Disable();
            }
        }

        foreach (var sourceGuardrail in source.Guardrails)
        {
            clone.SetGuardrail(sourceGuardrail.Period, sourceGuardrail.AmountUsd);
        }

        return clone;
    }
}
