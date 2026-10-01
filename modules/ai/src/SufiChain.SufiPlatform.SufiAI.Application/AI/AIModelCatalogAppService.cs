using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize]
public class AIModelCatalogAppService : SufiApplicationService, IAIModelCatalogAppService
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IAIModelRouteResolver _routeResolver;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly IAIHooshvareModelSelectionPolicyProvider _hooshvarePolicyProvider;
    private readonly IEnumerable<IAiProviderProfile> _profiles;
    private readonly IModelEndpointLookup _endpointLookup;

    public AIModelCatalogAppService(
        IWorkspaceRepository workspaceRepository,
        IAIModelRouteResolver routeResolver,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IAIHooshvareModelSelectionPolicyProvider hooshvarePolicyProvider,
        IEnumerable<IAiProviderProfile> profiles,
        IModelEndpointLookup endpointLookup)
    {
        _workspaceRepository = workspaceRepository;
        _routeResolver = routeResolver;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _hooshvarePolicyProvider = hooshvarePolicyProvider;
        _profiles = profiles;
        _endpointLookup = endpointLookup;
    }

    public virtual async Task<List<AIModelRouteDto>> GetSelectableRoutesAsync(GetSelectableModelRoutesInput input)
    {
        if (input.HooshvareId is not Guid)
        {
            await AuthorizationService.CheckAsync(AIPermissions.AI.Chat);
        }

        var workspace = await _workspaceRepository.GetAsync(input.WorkspaceId, includeDetails: true);
        IReadOnlyCollection<Guid>? allowlist = null;

        if (input.HooshvareId is Guid hooshvareId)
        {
            var policy = await _hooshvarePolicyProvider.FindAsync(hooshvareId);
            if (policy == null)
            {
                throw new BusinessException(AIErrorCodes.ModelRouteHooshvareNotFound)
                    .WithData("HooshvareId", hooshvareId);
            }

            if (policy.WorkspaceId != workspace.Id)
            {
                throw new BusinessException(AIErrorCodes.ModelRouteOutsideWorkspace)
                    .WithData("HooshvareId", hooshvareId)
                    .WithData("WorkspaceId", workspace.Id)
                    .WithData("ActualWorkspaceId", policy.WorkspaceId);
            }

            if (policy.AllowedModelConfigurationIds.Count > 0)
            {
                allowlist = policy.AllowedModelConfigurationIds;
            }
        }

        var implicitRoute = _runtimeConfigurationResolver.Resolve(workspace, input.CapabilityType);
        var snapshots = _routeResolver.ListSelectableRoutes(
            workspace,
            input.CapabilityType,
            allowlist);

        var routes = new List<AIModelRouteDto>(snapshots.Count);
        foreach (var snapshot in snapshots)
        {
            routes.Add(await MapRouteAsync(snapshot, implicitRoute));
        }

        return routes;
    }

    private async Task<AIModelRouteDto> MapRouteAsync(
        WorkspaceRuntimeConfiguration snapshot,
        WorkspaceRuntimeConfiguration implicitRoute)
    {
        var displayName = snapshot.ModelConfiguration?.DisplayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = snapshot.ModelId;
        }

        var profile = AiProviderProfiles.Find(_profiles, snapshot.Provider);
        var endpoints = await _endpointLookup.GetSupportedEndpointsAsync(snapshot.Provider, snapshot.ModelId);
        return new AIModelRouteDto
        {
            Id = snapshot.ModelConfigurationId ?? snapshot.ModelConfiguration?.Id ?? Guid.Empty,
            ModelId = snapshot.ModelId,
            DisplayName = displayName,
            Description = snapshot.ModelConfiguration?.Description,
            CapabilityType = snapshot.CapabilityType,
            OpenAIApiMode = snapshot.OpenAIApiMode,
            IsDefault = implicitRoute.ModelConfigurationId.HasValue &&
                        implicitRoute.ModelConfigurationId == snapshot.ModelConfigurationId,
            IsReady = snapshot.IsReady,
            UnavailableReason = snapshot.IsReady
                ? null
                : LocalizeUnavailableReason(snapshot.FailureCode),
            SupportsResponses = profile?.SupportsApiMode(OpenAIApiMode.Responses) == true &&
                                OpenAIApiModePolicy.HasResponsesEndpoint(endpoints),
            SupportsToolCalling = snapshot.IsReady &&
                                  snapshot.Provider is AIProviderType.OpenAI
                                      or AIProviderType.OpenAICompatible
                                      or AIProviderType.OpenRouter
                                      or AIProviderType.HuggingFace
                                      or AIProviderType.AvalAI
                                      or AIProviderType.Liara,
            AcceptsImageInput = snapshot.ModelConfiguration?.AcceptsImageInput,
            AcceptsFileInput = snapshot.ModelConfiguration?.AcceptsFileInput,
            SupportsReasoning = snapshot.ModelConfiguration?.SupportsReasoning,
            ReasoningEfforts = snapshot.ModelConfiguration?.ReasoningEfforts,
            DefaultReasoningEffort = snapshot.ModelConfiguration?.DefaultReasoningEffort
        };
    }

    private string? LocalizeUnavailableReason(string? failureCode)
    {
        if (string.IsNullOrWhiteSpace(failureCode))
        {
            return null;
        }

        var key = "RouteUnavailable:" + failureCode;
        var localized = L[key];
        return localized.ResourceNotFound ? failureCode : localized.Value;
    }
}
