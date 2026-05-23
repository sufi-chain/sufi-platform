using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using SufiChain.SufiAbp.PermissionManagement;

namespace SufiChain.SufiAbp.PermissionManagement.Controllers;


[Area(PermissionManagementRemoteServiceConsts.ModuleName)]
[RemoteService(Name = PermissionManagementRemoteServiceConsts.RemoteServiceName)]
[Route("api/sabp/permission-management/permissions")]
public class PermissionsController : SufiAbpControllerBase, IPermissionAppService
{
    private readonly IPermissionAppService _permissionAppService;

    public PermissionsController(IPermissionAppService permissionAppService)
    {
        _permissionAppService = permissionAppService;
    }

    [HttpGet]
    public virtual Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey)
    {
        return _permissionAppService.GetAsync(providerName, providerKey);
    }

    [HttpGet]
    [Route("by-group")]
    public virtual Task<GetPermissionListResultDto> GetByGroupAsync(string groupName, string providerName, string providerKey)
    {
        return _permissionAppService.GetByGroupAsync(groupName, providerName, providerKey);
    }

    [HttpPut]
    public virtual Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input)
    {
        return _permissionAppService.UpdateAsync(providerName, providerKey, input);
    }

    public virtual Task DeleteResourceAsync(string resourceName, string resourceKey, string providerName, string providerKey)
    {
        return _permissionAppService.DeleteResourceAsync(resourceName, resourceKey, providerName, providerKey);
    }

    public virtual Task<GetResourcePermissionListResultDto> GetResourceAsync(string resourceName, string resourceKey)
    {
        return _permissionAppService.GetResourceAsync(resourceName, resourceKey);
    }

    public virtual Task<GetResourcePermissionWithProviderListResultDto> GetResourceByProviderAsync(string resourceName, string resourceKey, string providerName, string providerKey)
    {
        return _permissionAppService.GetResourceByProviderAsync(resourceName, resourceKey, providerName, providerKey);
    }

    public virtual Task<GetResourcePermissionDefinitionListResultDto> GetResourceDefinitionsAsync(string resourceName)
    {
        return _permissionAppService.GetResourceDefinitionsAsync(resourceName);
    }

    public virtual Task<GetResourceProviderListResultDto> GetResourceProviderKeyLookupServicesAsync(string resourceName)
    {
        return _permissionAppService.GetResourceProviderKeyLookupServicesAsync(resourceName);
    }

    public virtual Task<SearchProviderKeyListResultDto> SearchResourceProviderKeyAsync(string resourceName, string serviceName, string filter, int page)
    {
        return _permissionAppService.SearchResourceProviderKeyAsync(resourceName, serviceName, filter, page);
    }

    public virtual Task UpdateResourceAsync(string resourceName, string resourceKey, UpdateResourcePermissionsDto input)
    {
        return _permissionAppService.UpdateResourceAsync(resourceName, resourceKey, input);
    }
}



//[Area(PermissionManagementRemoteServiceConsts.ModuleName)]
//[RemoteService(Name = PermissionManagementRemoteServiceConsts.RemoteServiceName)]
//[Route("api/sabp/permission-management/permissions")]
//public class PermissionsController : SufiAbpControllerBase, IPermissionAppService
//{
//    private readonly IPermissionAppService _permissionAppService;

//    public PermissionsController(IPermissionAppService permissionAppService)
//    {
//        _permissionAppService = permissionAppService;
//    }

//    //public Task DeleteResourceAsync(string resourceName, string resourceKey, string providerName, string providerKey)
//    //{
//    //    return _permissionAppService.DeleteResourceAsync(resourceName, resourceKey, providerName, providerKey);
//    //}

//    [HttpGet]
//    public virtual Task<GetPermissionListResultDto> GetAsync(string providerName, string providerKey)
//    {
//        return _permissionAppService.GetAsync(providerName, providerKey);
//    }

//    [HttpGet]
//    [Route("by-group")]
//    public virtual Task<GetPermissionListResultDto> GetByGroupAsync(string groupName, string providerName, string providerKey)
//    {
//        return _permissionAppService.GetByGroupAsync(groupName, providerName, providerKey);
//    }

//    public Task<GetResourcePermissionListResultDto> GetResourceAsync(string resourceName, string resourceKey)
//    {
//        return _permissionAppService.GetResourceAsync(resourceName, resourceKey);
//    }

//    public Task<GetResourcePermissionWithProviderListResultDto> GetResourceByProviderAsync(string resourceName, string resourceKey, string providerName, string providerKey)
//    {
//        return _permissionAppService.GetResourceByProviderAsync(resourceName, resourceKey, providerName, providerKey);
//    }

//    public Task<GetResourcePermissionDefinitionListResultDto> GetResourceDefinitionsAsync(string resourceName)
//    {
//        return _permissionAppService.GetResourceDefinitionsAsync(resourceName);
//    }

//    public Task<GetResourceProviderListResultDto> GetResourceProviderKeyLookupServicesAsync(string resourceName)
//    {
//        return _permissionAppService.GetResourceProviderKeyLookupServicesAsync(resourceName);
//    }

//    public Task<SearchProviderKeyListResultDto> SearchResourceProviderKeyAsync(string resourceName, string serviceName, string filter, int page)
//    {
//        return _permissionAppService.SearchResourceProviderKeyAsync(resourceName, serviceName, filter, page);
//    }

//    [HttpPut]
//    public virtual Task UpdateAsync(string providerName, string providerKey, UpdatePermissionsDto input)
//    {
//        return _permissionAppService.UpdateAsync(providerName, providerKey, input);
//    }

//    public Task UpdateResourceAsync(string resourceName, string resourceKey, UpdateResourcePermissionsDto input)
//    {
//        return _permissionAppService.UpdateResourceAsync(resourceName, resourceKey, input);
//    }
//}
