using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;

namespace SufiChain.SufiAbp.FeatureManagement;

[Authorize]
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IFeatureAppService))]
public class FeatureAppService : SufiAbpApplicationService, IFeatureAppService
{
    public const string HostOnlyPropertyKey = "HostOnly";

    protected FeatureManagementOptions Options { get; }
    protected IFeatureManager FeatureManager { get; }
    protected IFeatureDefinitionManager FeatureDefinitionManager { get; }

    public FeatureAppService(
        IFeatureManager featureManager,
        IFeatureDefinitionManager featureDefinitionManager,
        IOptions<FeatureManagementOptions> options)
    {
        FeatureManager = featureManager;
        FeatureDefinitionManager = featureDefinitionManager;
        Options = options.Value;
    }

    public virtual async Task<GetFeatureListResultDto> GetAsync([NotNull] string providerName, string? providerKey)
    {
        await CheckProviderPolicyAsync(providerName, providerKey);

        var result = new GetFeatureListResultDto
        {
            Groups = new List<FeatureGroupDto>()
        };

        var hostOnlyNames = new HashSet<string>();
        if (providerName == TenantFeatureValueProvider.ProviderName && !string.IsNullOrEmpty(providerKey))
        {
            hostOnlyNames = await GetHostOnlyFeatureNamesAsync();
        }

        foreach (var group in await FeatureDefinitionManager.GetGroupsAsync())
        {
            var groupDto = new FeatureGroupDto
            {
                Name = group.Name,
                DisplayName = group.DisplayName?.Localize(StringLocalizerFactory),
                Features = new List<FeatureDto>()
            };

            foreach (var featureDefinition in group.GetFeaturesWithChildren())
            {
                if (providerName == TenantFeatureValueProvider.ProviderName &&
                    CurrentTenant.Id == null &&
                    providerKey == null &&
                    !featureDefinition.IsAvailableToHost)
                {
                    continue;
                }

                if (hostOnlyNames.Contains(featureDefinition.Name))
                {
                    continue;
                }

                var feature = await FeatureManager.GetOrNullWithProviderAsync(
                    featureDefinition.Name,
                    providerName,
                    providerKey
                );

                groupDto.Features.Add(CreateFeatureDto(feature, featureDefinition));
            }

            SetFeatureDepth(groupDto.Features);

            if (groupDto.Features.Any())
            {
                result.Groups.Add(groupDto);
            }
        }

        return result;
    }

    public virtual async Task UpdateAsync([NotNull] string providerName, string? providerKey, UpdateFeaturesDto input)
    {
        await CheckProviderPolicyAsync(providerName, providerKey);

        var inputFeatures = input.Features;
        if (providerName == TenantFeatureValueProvider.ProviderName && !string.IsNullOrEmpty(providerKey))
        {
            var hostOnlyNames = await GetHostOnlyFeatureNamesAsync();
            inputFeatures = inputFeatures
                .Where(feature => !hostOnlyNames.Contains(feature.Name))
                .ToList();
        }

        var inputFeatureNames = inputFeatures.Select(feature => feature.Name).ToHashSet();
        var featureMap = inputFeatures.ToDictionary(feature => feature.Name);
        var features = new Dictionary<UpdateFeatureDto, List<UpdateFeatureDto>>();
        var processed = new HashSet<string>();

        foreach (var feature in inputFeatures)
        {
            if (!processed.Add(feature.Name))
            {
                continue;
            }

            var featureDefinition = await FeatureDefinitionManager.GetAsync(feature.Name);
            var validChildren = new List<UpdateFeatureDto>();

            foreach (var childFeature in featureDefinition.Children)
            {
                if (inputFeatureNames.Contains(childFeature.Name) &&
                    featureMap.TryGetValue(childFeature.Name, out var childDto) &&
                    processed.Add(childFeature.Name))
                {
                    validChildren.Add(childDto);
                }
            }

            features[feature] = validChildren;
        }

        foreach (var feature in features)
        {
            var forceToSet = false;

            foreach (var childFeature in feature.Value)
            {
                await FeatureManager.SetAsync(childFeature.Name, childFeature.Value, providerName, providerKey);
                var value = await FeatureManager.GetOrNullWithProviderAsync(childFeature.Name, providerName, providerKey);
                if (value.Provider?.Name == providerName && value.Provider?.Key == providerKey)
                {
                    forceToSet = true;
                }
            }

            await FeatureManager.SetAsync(
                feature.Key.Name,
                feature.Key.Value,
                providerName,
                providerKey,
                forceToSet: forceToSet
            );
        }
    }

    public virtual async Task DeleteAsync([NotNull] string providerName, string? providerKey)
    {
        await CheckProviderPolicyAsync(providerName, providerKey);
        await FeatureManager.DeleteAsync(providerName, providerKey);
    }

    protected virtual void SetFeatureDepth(List<FeatureDto> features, FeatureDto? parentFeature = null, int depth = 0)
    {
        foreach (var feature in features)
        {
            if ((parentFeature == null && feature.ParentName == null) ||
                (parentFeature != null && parentFeature.Name == feature.ParentName))
            {
                feature.Depth = depth;
                SetFeatureDepth(features, feature, depth + 1);
            }
        }
    }

    protected virtual async Task CheckProviderPolicyAsync(string providerName, string? providerKey)
    {
        string? policyName;

        if (providerName == TenantFeatureValueProvider.ProviderName && CurrentTenant.Id == null && providerKey == null)
        {
            policyName = FeatureManagementPermissions.Features.ManageHostFeatures;
        }
        else
        {
            policyName = Options.ProviderPolicies.GetOrDefault(providerName);
            if (policyName.IsNullOrEmpty())
            {
                throw new AbpException(
                    $"No policy defined to get/set permissions for the provider '{providerName}'. Use {nameof(FeatureManagementOptions)} to map the policy."
                );
            }
        }

        await AuthorizationService.CheckAsync(policyName);
    }

    private async Task<HashSet<string>> GetHostOnlyFeatureNamesAsync()
    {
        var hostOnly = new HashSet<string>();

        foreach (var group in await FeatureDefinitionManager.GetGroupsAsync())
        {
            foreach (var feature in group.GetFeaturesWithChildren())
            {
                if (feature.Properties.GetOrDefault(HostOnlyPropertyKey) is true)
                {
                    hostOnly.Add(feature.Name);
                }
            }
        }

        return hostOnly;
    }

    private FeatureDto CreateFeatureDto(
        FeatureNameValueWithGrantedProvider featureNameValueWithGrantedProvider,
        FeatureDefinition featureDefinition)
    {
        return new FeatureDto
        {
            Name = featureDefinition.Name,
            DisplayName = featureDefinition.DisplayName?.Localize(StringLocalizerFactory),
            Description = featureDefinition.Description?.Localize(StringLocalizerFactory),
            ValueType = featureDefinition.ValueType,
            ParentName = featureDefinition.Parent?.Name,
            Value = featureNameValueWithGrantedProvider.Value,
            Provider = new FeatureProviderDto
            {
                Name = featureNameValueWithGrantedProvider.Provider?.Name,
                Key = featureNameValueWithGrantedProvider.Provider?.Key
            }
        };
    }
}
