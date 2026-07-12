using Volo.Abp.Application.Services;
using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantAppService : ISufiCrudAppService<TenantDto, Guid, GetTenantsInput, TenantCreateDto, TenantUpdateDto>
{
    Task<string> GetDefaultConnectionStringAsync(Guid id);

    Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString);

    Task DeleteDefaultConnectionStringAsync(Guid id);
}
