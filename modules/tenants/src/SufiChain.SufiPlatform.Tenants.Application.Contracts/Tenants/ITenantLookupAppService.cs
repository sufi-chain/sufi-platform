using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Tenants.Tenants;

/// <summary>
/// Application service for anonymous tenant lookup (e.g. tenant selector on login/register).
/// Returns only Id and Name for tenant selection.
/// </summary>
public interface ITenantLookupAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of tenants for lookup.
    /// Allowed anonymously so unauthenticated users can select a tenant on the login page.
    /// </summary>
    [AllowAnonymous]
    Task<PagedResultDto<TenantLookupItemDto>> GetListAsync(GetTenantLookupInput input);
}
