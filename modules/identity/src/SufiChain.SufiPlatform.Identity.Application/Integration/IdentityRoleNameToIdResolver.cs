using SufiChain.SufiPlatform.FileManager.AccessControl;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Identity.Integration;

/// <summary>
/// Maps the current user's role names to Identity role ids in the current tenant.
/// Results are cached for the request scope, keyed by tenant, so a tenant switch
/// cannot reuse another tenant's role id.
/// </summary>
public class IdentityRoleNameToIdResolver : IRoleNameToIdResolver
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly Dictionary<(Guid? TenantId, string NormalizedName), Guid?> _cache = new();

    public IdentityRoleNameToIdResolver(
        IIdentityRoleRepository roleRepository,
        ICurrentTenant currentTenant)
    {
        _roleRepository = roleRepository;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<Guid>> ResolveAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        if (roleNames == null)
        {
            return Array.Empty<Guid>();
        }

        var tenantId = _currentTenant.Id;
        var ids = new List<Guid>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roleNames)
        {
            if (string.IsNullOrWhiteSpace(roleName) || !seen.Add(roleName))
            {
                continue;
            }

            var normalized = roleName.ToUpperInvariant();
            var key = (tenantId, normalized);
            if (!_cache.TryGetValue(key, out var roleId))
            {
                var role = await _roleRepository.FindByNormalizedNameAsync(
                    normalized,
                    includeDetails: false,
                    cancellationToken: cancellationToken);
                roleId = role?.Id;
                _cache[key] = roleId;
            }

            if (roleId.HasValue)
            {
                ids.Add(roleId.Value);
            }
        }

        return ids;
    }
}
