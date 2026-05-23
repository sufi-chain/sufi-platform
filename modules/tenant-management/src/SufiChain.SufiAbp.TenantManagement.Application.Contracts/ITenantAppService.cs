using Volo.Abp.Application.Services;
using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.TenantManagement;

public interface ITenantAppService : ISufiAbpCrudAppService<TenantDto, Guid, GetTenantsInput, TenantCreateDto, TenantUpdateDto>
{
    Task<string> GetDefaultConnectionStringAsync(Guid id);

    Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString);

    Task DeleteDefaultConnectionStringAsync(Guid id);
}
