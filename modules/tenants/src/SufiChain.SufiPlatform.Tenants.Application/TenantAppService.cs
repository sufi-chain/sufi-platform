using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiPlatform.Tenants;

[Authorize(TenantsPermissions.Tenants.Default)]
public class TenantAppService : TenantsAppServiceBase, ITenantAppService
{
    protected ITenantRepository TenantRepository { get; }
    protected ITenantManager TenantManager { get; }
    protected IDataSeeder DataSeeder { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected TenantDedicatedSchemaMigrator DedicatedSchemaMigrator { get; }

    public TenantAppService(
        ITenantRepository tenantRepository,
        ITenantManager tenantManager,
        IDataSeeder dataSeeder,
        IDistributedEventBus distributedEventBus,
        TenantDedicatedSchemaMigrator dedicatedSchemaMigrator)
    {
        TenantRepository = tenantRepository;
        TenantManager = tenantManager;
        DataSeeder = dataSeeder;
        DistributedEventBus = distributedEventBus;
        DedicatedSchemaMigrator = dedicatedSchemaMigrator;
    }

    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual async Task<TenantDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Default);
        return MapToDto(await TenantRepository.GetAsync(id, includeDetails: true));
    }

    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Default);
        var count = await TenantRepository.GetCountAsync(input.Filter);
        var tenants = await TenantRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter,
            includeDetails: true);

        return new PagedResultDto<TenantDto>(count, tenants.Select(MapToDto).ToList());
    }

    [Authorize(TenantsPermissions.Tenants.Create)]
    public virtual async Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Create);
        var tenant = await TenantManager.CreateAsync(input.Name);
        tenant.SetEditionId(input.EditionId);
        tenant.SetOwnerUserId(input.OwnerUserId);
        await ConfigureRoutingAsync(tenant, input);
        await TenantRepository.InsertAsync(tenant, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        // The outbox delivers TenantCreatedEto after this unit of work. Migrate a
        // dedicated database first so the admin seed writes into that database.
        await DedicatedSchemaMigrator.MigrateAsync(tenant.Id);

        using (CurrentTenant.Change(tenant.Id, tenant.Name))
        {
            await DataSeeder.SeedAsync(
                new DataSeedContext(tenant.Id)
                    .WithProperty(TenantCreatedEto.AdminEmailPropertyName, input.AdminEmailAddress)
                    .WithProperty(TenantCreatedEto.AdminPasswordPropertyName, input.AdminPassword));
        }

        // Default publish flags: the host outbox stores this event. The password
        // stays on the seed context above and is not a property of the event.
        await DistributedEventBus.PublishAsync(
            new TenantCreatedEto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Properties =
                {
                    [TenantCreatedEto.AdminEmailPropertyName] = input.AdminEmailAddress
                }
            });

        return MapToDto(tenant);
    }

    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual async Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Update);
        var tenant = await TenantRepository.GetAsync(id, includeDetails: true);
        tenant.ConcurrencyStamp = input.ConcurrencyStamp;
        await TenantManager.ChangeNameAsync(tenant, input.Name);
        tenant.SetEditionId(input.EditionId);
        tenant.SetOwnerUserId(input.OwnerUserId);
        await ConfigureRoutingAsync(tenant, input);
        await TenantRepository.UpdateAsync(tenant, autoSave: true);
        return MapToDto(tenant);
    }

    [Authorize(TenantsPermissions.Tenants.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Delete);
        await TenantRepository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual async Task ActivateAsync(Guid id)
    {
        await SetActiveAsync(id, active: true);
    }

    [Authorize(TenantsPermissions.Tenants.Update)]
    public virtual async Task DeactivateAsync(Guid id)
    {
        await SetActiveAsync(id, active: false);
    }

    protected virtual async Task SetActiveAsync(Guid id, bool active)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.Update);
        var tenant = await TenantRepository.GetAsync(id);
        if (active)
        {
            tenant.Activate();
        }
        else
        {
            tenant.Deactivate();
        }

        await TenantRepository.UpdateAsync(tenant, autoSave: true);
        var cache = LazyServiceProvider.LazyGetRequiredService<IDistributedCache<TenantConfigurationCacheItem>>();
        await cache.RemoveManyAsync(
            new[]
            {
                TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id, null),
                TenantConfigurationCacheItem.CalculateCacheKey(null, tenant.NormalizedName),
                TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id, tenant.NormalizedName),
            },
            considerUow: true);
    }

    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual async Task<string> GetDefaultConnectionStringAsync(Guid id)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.ManageConnectionStrings);
        var tenant = await TenantRepository.GetAsync(id);
        return tenant.FindDefaultConnectionString() ?? string.Empty;
    }

    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual async Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.ManageConnectionStrings);
        var tenant = await TenantRepository.GetAsync(id);
        tenant.SetDefaultConnectionString(defaultConnectionString);
        await TenantRepository.UpdateAsync(tenant, autoSave: true);
    }

    [Authorize(TenantsPermissions.Tenants.ManageConnectionStrings)]
    public virtual async Task DeleteDefaultConnectionStringAsync(Guid id)
    {
        await CheckPolicyAsync(TenantsPermissions.Tenants.ManageConnectionStrings);
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
            IsActive = tenant.IsActive,
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
