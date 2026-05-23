using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.PermissionManagement.Localization;

namespace SufiChain.SufiAbp.PermissionManagement;

[Microsoft.AspNetCore.Authorization.Authorize]
public class PermissionAppService : SufiAbpApplicationService, IPermissionAppService
{
    protected PermissionManagementOptions Options { get; }
    protected IPermissionManager PermissionManager { get; }
    protected IResourcePermissionManager ResourcePermissionManager { get; }

    public PermissionAppService(
        IPermissionManager permissionManager,
        IResourcePermissionManager resourcePermissionManager,
        IOptions<PermissionManagementOptions> options)
    {
        LocalizationResource = typeof(SufiAbpPermissionManagementResource);
        ObjectMapperContext = typeof(SufiAbpPermissionManagementApplicationModule);

        Options = options.Value;
        PermissionManager = permissionManager;
        ResourcePermissionManager = resourcePermissionManager;
    }

    public virtual async Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey)
    {
        return await GetByGroupAsync(null, providerName, providerKey);
    }

    public virtual async Task<GetPermissionListResultDto> GetByGroupAsync(string groupName, string providerName, string providerKey)
    {
        await CheckProviderPolicyAsync(providerName);

        var result = new GetPermissionListResultDto
        {
            EntityDisplayName = providerKey,
            Groups = new List<PermissionGroupDto>()
        };

        var grants = await PermissionManager.GetAllAsync(providerName, providerKey);
        if (grants.Count == 0)
        {
            return result;
        }

        result.Groups.Add(new PermissionGroupDto
        {
            Name = string.IsNullOrWhiteSpace(groupName) ? PermissionManagementRemoteServiceConsts.ModuleName : groupName,
            DisplayName = string.IsNullOrWhiteSpace(groupName) ? L["Permissions"].ToString() : groupName,
            DisplayNameKey = null,
            DisplayNameResource = null,
            Permissions = grants.Select(grant => new PermissionGrantInfoDto
            {
                Name = grant.Name,
                DisplayName = grant.Name,
                ParentName = null,
                IsGranted = grant.IsGranted,
                AllowedProviders = new List<string> { providerName },
                GrantedProviders = grant.Providers.Select(provider => new ProviderInfoDto
                {
                    ProviderName = provider.Name,
                    ProviderKey = provider.Key
                }).ToList()
            }).ToList()
        });

        return result;
    }

    public virtual async Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input)
    {
        await CheckProviderPolicyAsync(providerName);

        foreach (var permissionDto in input.Permissions ?? Array.Empty<UpdatePermissionDto>())
        {
            await PermissionManager.SetAsync(permissionDto.Name, providerName, providerKey, permissionDto.IsGranted);
        }
    }

    public virtual async Task<GetResourceProviderListResultDto> GetResourceProviderKeyLookupServicesAsync(string resourceName)
    {
        var lookupServices = await ResourcePermissionManager.GetProviderKeyLookupServicesAsync();
        return new GetResourceProviderListResultDto
        {
            Providers = lookupServices.Select(service => new ResourceProviderDto
            {
                Name = service.Name,
                DisplayName = service.DisplayName.ToString()
            }).ToList()
        };
    }

    public virtual async Task<SearchProviderKeyListResultDto> SearchResourceProviderKeyAsync(string resourceName, string serviceName, string filter, int page)
    {
        var lookupService = await ResourcePermissionManager.GetProviderKeyLookupServiceAsync(serviceName);
        var keys = await lookupService.SearchAsync(filter, page);
        return new SearchProviderKeyListResultDto
        {
            Keys = keys.Select(x => new SearchProviderKeyInfo
            {
                ProviderKey = x.ProviderKey,
                ProviderDisplayName = x.ProviderDisplayName,
            }).ToList()
        };
    }

    public virtual Task<GetResourcePermissionDefinitionListResultDto> GetResourceDefinitionsAsync(string resourceName)
    {
        return Task.FromResult(new GetResourcePermissionDefinitionListResultDto
        {
            Permissions = new List<ResourcePermissionDefinitionDto>()
        });
    }

    public virtual async Task<GetResourcePermissionListResultDto> GetResourceAsync(string resourceName, string resourceKey)
    {
        var resourcePermissionGrants = await ResourcePermissionManager.GetAllGroupAsync(resourceName, resourceKey);
        return new GetResourcePermissionListResultDto
        {
            Permissions = resourcePermissionGrants.Select(grant => new ResourcePermissionGrantInfoDto
            {
                ProviderName = grant.ProviderName,
                ProviderKey = grant.ProviderKey,
                ProviderDisplayName = grant.ProviderDisplayName,
                ProviderNameDisplayName = grant.ProviderNameDisplayName?.ToString(),
                Permissions = grant.Permissions.Select(permission => new GrantedResourcePermissionDto
                {
                    Name = permission,
                    DisplayName = permission
                }).ToList()
            }).ToList()
        };
    }

    public virtual async Task<GetResourcePermissionWithProviderListResultDto> GetResourceByProviderAsync(string resourceName, string resourceKey, string providerName, string providerKey)
    {
        var grants = await ResourcePermissionManager.GetAllAsync(resourceName, resourceKey, providerName, providerKey);
        return new GetResourcePermissionWithProviderListResultDto
        {
            Permissions = grants.Select(grant => new ResourcePermissionWithProdiverGrantInfoDto
            {
                Name = grant.Name,
                DisplayName = grant.Name,
                Providers = grant.Providers.Select(provider => provider.Name).ToList(),
                IsGranted = grant.IsGranted
            }).ToList()
        };
    }

    public virtual async Task UpdateResourceAsync(string resourceName, string resourceKey, UpdateResourcePermissionsDto input)
    {
        foreach (var permissionName in input.Permissions ?? new List<string>())
        {
            await ResourcePermissionManager.SetAsync(permissionName, resourceName, resourceKey, input.ProviderName, input.ProviderKey, true);
        }
    }

    public virtual Task DeleteResourceAsync(string resourceName, string resourceKey, string providerName, string providerKey)
    {
        return ResourcePermissionManager.DeleteAsync(resourceName, resourceKey, providerName, providerKey);
    }

    protected virtual async Task CheckProviderPolicyAsync(string providerName)
    {
        if (!Options.ProviderPolicies.TryGetValue(providerName, out var policyName) || string.IsNullOrWhiteSpace(policyName))
        {
            throw new InvalidOperationException($"No policy defined to get/set permissions for provider '{providerName}'.");
        }

        await CheckPolicyAsync(policyName);
    }
}
