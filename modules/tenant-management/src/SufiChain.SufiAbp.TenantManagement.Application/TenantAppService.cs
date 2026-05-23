using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.TenantManagement;

public class TenantAppService : TenantManagementAppServiceBase, ITenantAppService
{
    protected ITenantRepository TenantRepository { get; }
    protected ITenantManager TenantManager { get; }

    public TenantAppService(ITenantRepository tenantRepository, ITenantManager tenantManager)
    {
        TenantRepository = tenantRepository;
        TenantManager = tenantManager;
    }

    public virtual async Task<TenantDto> GetAsync(Guid id)
    {
        return MapToDto(await TenantRepository.GetAsync(id));
    }

    public virtual async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input)
    {
        var count = await TenantRepository.GetCountAsync(input.Filter);
        var tenants = await TenantRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter);

        return new PagedResultDto<TenantDto>(count, tenants.Select(MapToDto).ToList());
    }

    public virtual async Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        var tenant = await TenantManager.CreateAsync(input.Name);
        await TenantRepository.InsertAsync(tenant, autoSave: true);
        return MapToDto(tenant);
    }

    public virtual async Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        var tenant = await TenantRepository.GetAsync(id);
        tenant.ConcurrencyStamp = input.ConcurrencyStamp;
        await TenantManager.ChangeNameAsync(tenant, input.Name);
        await TenantRepository.UpdateAsync(tenant, autoSave: true);
        return MapToDto(tenant);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await TenantRepository.DeleteAsync(id, autoSave: true);
    }

    public virtual async Task<string> GetDefaultConnectionStringAsync(Guid id)
    {
        var tenant = await TenantRepository.GetAsync(id);
        return tenant.FindDefaultConnectionString() ?? string.Empty;
    }

    public virtual async Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString)
    {
        var tenant = await TenantRepository.GetAsync(id);
        tenant.SetDefaultConnectionString(defaultConnectionString);
        await TenantRepository.UpdateAsync(tenant, autoSave: true);
    }

    public virtual async Task DeleteDefaultConnectionStringAsync(Guid id)
    {
        var tenant = await TenantRepository.GetAsync(id);
        tenant.RemoveDefaultConnectionString();
        await TenantRepository.UpdateAsync(tenant, autoSave: true);
    }

    protected virtual TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            ConcurrencyStamp = tenant.ConcurrencyStamp
        };
    }
}
