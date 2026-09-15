using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIModelRouteResolver : IAIModelRouteResolver
{
    public const string DisabledRule = "Disabled";
    public const string NotUserSelectableRule = "NotUserSelectable";

    protected IWorkspaceRepository WorkspaceRepository { get; }

    protected IAIModelConfigurationRepository ConfigurationRepository { get; }

    protected IWorkspaceRuntimeConfigurationResolver RuntimeConfigurationResolver { get; }

    public AIModelRouteResolver(
        IWorkspaceRepository workspaceRepository,
        IAIModelConfigurationRepository configurationRepository,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver)
    {
        WorkspaceRepository = workspaceRepository;
        ConfigurationRepository = configurationRepository;
        RuntimeConfigurationResolver = runtimeConfigurationResolver;
    }

    public virtual async Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default)
    {
        var workspace = await WorkspaceRepository.FindAsync(
            workspaceId,
            includeDetails: true,
            cancellationToken: cancellationToken);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceId", workspaceId);
        }

        if (selection.ModelConfigurationId is not Guid modelConfigurationId)
        {
            return RuntimeConfigurationResolver.Resolve(workspace, capabilityType);
        }

        var configuration = workspace.ModelConfigurations
            .FirstOrDefault(item => item.Id == modelConfigurationId);
        if (configuration == null)
        {
            configuration = await ConfigurationRepository.FindAsync(
                modelConfigurationId,
                includeDetails: true,
                cancellationToken: cancellationToken);
        }

        return ResolveExplicit(workspace, capabilityType, configuration, selection, modelConfigurationId);
    }

    public virtual WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection)
    {
        if (selection.ModelConfigurationId is not Guid modelConfigurationId)
        {
            return RuntimeConfigurationResolver.Resolve(workspace, capabilityType);
        }

        var configuration = workspace.ModelConfigurations
            .FirstOrDefault(item => item.Id == modelConfigurationId);
        return ResolveExplicit(workspace, capabilityType, configuration, selection, modelConfigurationId);
    }

    public virtual IReadOnlyList<WorkspaceRuntimeConfiguration> ListSelectableRoutes(
        Workspace workspace,
        AICapabilityType capabilityType,
        IReadOnlyCollection<Guid>? allowedModelConfigurationIds = null)
    {
        var allowlist = allowedModelConfigurationIds is { Count: > 0 }
            ? allowedModelConfigurationIds
            : null;

        return workspace.ModelConfigurations
            .Where(configuration =>
                configuration.CapabilityType == capabilityType &&
                configuration.IsEnabled &&
                configuration.IsUserSelectable &&
                (allowlist == null || allowlist.Contains(configuration.Id)))
            .OrderBy(configuration => configuration.Priority)
            .Select(configuration => RuntimeConfigurationResolver.Resolve(
                workspace,
                capabilityType,
                configuration))
            .ToList();
    }

    protected virtual WorkspaceRuntimeConfiguration ResolveExplicit(
        Workspace workspace,
        AICapabilityType capabilityType,
        AIModelConfiguration? configuration,
        AIModelRouteSelection selection,
        Guid modelConfigurationId)
    {
        if (configuration == null)
        {
            throw new BusinessException(AIErrorCodes.ModelRouteNotFound)
                .WithData("ModelConfigurationId", modelConfigurationId)
                .WithData("WorkspaceId", workspace.Id);
        }

        if (configuration.WorkspaceId != workspace.Id)
        {
            throw new BusinessException(AIErrorCodes.ModelRouteOutsideWorkspace)
                .WithData("ModelConfigurationId", configuration.Id)
                .WithData("WorkspaceId", workspace.Id)
                .WithData("ActualWorkspaceId", configuration.WorkspaceId);
        }

        if (!configuration.IsEnabled)
        {
            throw CreateNotSelectable(configuration, DisabledRule);
        }

        if (configuration.CapabilityType != capabilityType)
        {
            throw new BusinessException(AIErrorCodes.ModelRouteCapabilityMismatch)
                .WithData("ModelConfigurationId", configuration.Id)
                .WithData("ExpectedCapabilityType", capabilityType.ToString())
                .WithData("ActualCapabilityType", configuration.CapabilityType.ToString());
        }

        if (!configuration.IsUserSelectable)
        {
            throw CreateNotSelectable(configuration, NotUserSelectableRule);
        }

        if (selection.AllowedModelConfigurationIds is { Count: > 0 } &&
            !selection.AllowedModelConfigurationIds.Contains(configuration.Id))
        {
            throw new BusinessException(AIErrorCodes.ModelRouteNotAllowedForCopilot)
                .WithData("ModelConfigurationId", configuration.Id)
                .WithData("WorkspaceId", workspace.Id);
        }

        if (selection.RequiresToolCalling &&
            configuration.OpenAIApiMode != OpenAIApiMode.ChatCompletions)
        {
            throw new BusinessException(AIErrorCodes.ModelRouteRequiresChatCompletions)
                .WithData("ModelConfigurationId", configuration.Id)
                .WithData("ApiMode", configuration.OpenAIApiMode.ToString());
        }

        return RuntimeConfigurationResolver.Resolve(
            workspace,
            capabilityType,
            configuration,
            isExplicitSelection: true);
    }

    protected virtual BusinessException CreateNotSelectable(
        AIModelConfiguration configuration,
        string rule)
    {
        return new BusinessException(AIErrorCodes.ModelRouteNotSelectable)
            .WithData("ModelConfigurationId", configuration.Id)
            .WithData("Rule", rule);
    }
}
