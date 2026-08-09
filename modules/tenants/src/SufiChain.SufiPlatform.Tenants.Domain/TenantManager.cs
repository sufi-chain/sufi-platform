using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantManager : DomainService, ITenantManager
{
    protected ITenantValidator TenantValidator { get; }
    protected ITenantNormalizer TenantNormalizer { get; }
    protected ILocalEventBus LocalEventBus { get; }
    protected ITenantRepository TenantRepository { get; }

    public TenantManager(
        ITenantValidator tenantValidator,
        ITenantNormalizer tenantNormalizer,
        ILocalEventBus localEventBus,
        ITenantRepository tenantRepository)
    {
        TenantValidator = tenantValidator;
        TenantNormalizer = tenantNormalizer;
        LocalEventBus = localEventBus;
        TenantRepository = tenantRepository;
    }

    public virtual async Task<Tenant> CreateAsync(string name)
    {
        Check.NotNull(name, nameof(name));

        var tenant = new Tenant(GuidGenerator.Create(), name, TenantNormalizer.NormalizeName(name));
        await TenantValidator.ValidateAsync(tenant);
        return tenant;
    }

    public virtual async Task<Tenant> CreateAsync(Guid id, string name)
    {
        Check.NotNull(name, nameof(name));

        var tenant = new Tenant(id, name, TenantNormalizer.NormalizeName(name));
        await TenantValidator.ValidateAsync(tenant);
        return tenant;
    }

    public virtual async Task ChangeNameAsync(Tenant tenant, string name)
    {
        Check.NotNull(tenant, nameof(tenant));
        Check.NotNull(name, nameof(name));

        await LocalEventBus.PublishAsync(new TenantChangedEvent(tenant.Id, tenant.NormalizedName));

        tenant.SetName(name);
        tenant.SetNormalizedName( TenantNormalizer.NormalizeName(name));
        await TenantValidator.ValidateAsync(tenant);
    }

    public virtual async Task SetDatabaseNameAsync(Tenant tenant, string databaseName)
    {
        Check.NotNull(tenant, nameof(tenant));
        Check.NotNullOrWhiteSpace(databaseName, nameof(databaseName));

        var existingTenant = await TenantRepository.FindByDatabaseNameAsync(databaseName);
        if (existingTenant != null && existingTenant.Id != tenant.Id)
        {
            throw new BusinessException("TenantManagement:DuplicateDatabaseName")
                .WithData("DatabaseName", databaseName);
        }

        tenant.SetDatabaseName(databaseName);
    }

    public virtual async Task ConfigureRoutingAsync(
        Tenant tenant,
        string primarySubdomain,
        IEnumerable<TenantDomainConfiguration> domains)
    {
        Check.NotNull(tenant, nameof(tenant));
        Check.NotNull(domains, nameof(domains));

        var normalizedSubdomain = TenantDomainName.NormalizeSubdomain(primarySubdomain);
        var existingSubdomainTenant = await TenantRepository.FindByPrimarySubdomainAsync(normalizedSubdomain);
        if (existingSubdomainTenant != null && existingSubdomainTenant.Id != tenant.Id)
        {
            throw new BusinessException("TenantManagement:DuplicatePrimarySubdomain")
                .WithData("PrimarySubdomain", normalizedSubdomain);
        }

        var normalizedDomains = domains
            .Select(domain => new TenantDomainConfiguration(
                TenantDomainName.NormalizeHost(domain.Host),
                domain.Type,
                domain.IsVerified,
                domain.IsActive))
            .GroupBy(domain => domain.Host, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        foreach (var domain in normalizedDomains)
        {
            var existingDomainTenant = await TenantRepository.FindByDomainHostAsync(domain.Host);
            if (existingDomainTenant != null && existingDomainTenant.Id != tenant.Id)
            {
                throw new BusinessException("TenantManagement:DuplicateDomainHost")
                    .WithData("Host", domain.Host);
            }
        }

        tenant.ConfigureRouting(
            normalizedSubdomain,
            normalizedDomains.Select(domain => new TenantDomain(
                GuidGenerator.Create(),
                tenant.Id,
                domain.Host,
                domain.Type,
                domain.IsVerified,
                domain.IsActive)));
    }
}
