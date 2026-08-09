using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantAppService : TenantsAppServiceBase, ITenantAppService
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
        return MapToDto(await TenantRepository.GetAsync(id, includeDetails: true));
    }

    public virtual async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input)
    {
        var count = await TenantRepository.GetCountAsync(input.Filter);
        var tenants = await TenantRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter,
            includeDetails: true);

        return new PagedResultDto<TenantDto>(count, tenants.Select(MapToDto).ToList());
    }

    public virtual async Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        var tenant = await TenantManager.CreateAsync(input.Name);
        tenant.SetEditionId(input.EditionId);
        tenant.SetOwnerUserId(input.OwnerUserId);
        await ConfigureRoutingAsync(tenant, input);
        await TenantRepository.InsertAsync(tenant, autoSave: true);
        return MapToDto(tenant);
    }

    public virtual async Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        var tenant = await TenantRepository.GetAsync(id, includeDetails: true);
        tenant.ConcurrencyStamp = input.ConcurrencyStamp;
        await TenantManager.ChangeNameAsync(tenant, input.Name);
        tenant.SetEditionId(input.EditionId);
        tenant.SetOwnerUserId(input.OwnerUserId);
        await ConfigureRoutingAsync(tenant, input);
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
            EditionId = tenant.EditionId,
            OwnerUserId = tenant.OwnerUserId,
            DatabaseName = tenant.DatabaseName,
            PrimarySubdomain = tenant.PrimarySubdomain,
            Domains = tenant.Domains.Select(domain => new TenantDomainDto
            {
                Id = domain.Id,
                Host = domain.Host,
                Type = domain.Type,
                IsVerified = domain.IsVerified,
                IsActive = domain.IsActive
            }).ToList(),
            ConcurrencyStamp = tenant.ConcurrencyStamp
        };
    }

    protected virtual async Task ConfigureRoutingAsync(
        Tenant tenant,
        TenantCreateOrUpdateDtoBase input)
    {
        if (input.PrimarySubdomain.IsNullOrWhiteSpace())
        {
            return;
        }

        await TenantManager.ConfigureRoutingAsync(
            tenant,
            input.PrimarySubdomain,
            input.Domains.Select(domain => new TenantDomainConfiguration(
                domain.Host,
                domain.Type,
                domain.IsVerified,
                domain.IsActive)));
    }
}
