using SufiChain.SufiPlatform.Identity.OrganizationUnits;
using SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

namespace SufiChain.SufiPlatform.Identity.Blazor.Public.Components;

public abstract class SufiOrganizationUnitLookupInlineComponentBase : IdentityPublicComponentBase
{
    protected const int DefaultMaxResultCount = 20;

    protected IOrganizationUnitAppService OrganizationUnitAppService =>
        LazyGetRequiredService(ref _organizationUnitAppService);

    private IOrganizationUnitAppService? _organizationUnitAppService;

    private List<OrganizationUnitDto>? _cachedTree;

    protected static string FormatOrganizationUnitLabel(OrganizationUnitDto organizationUnit)
    {
        return $"({organizationUnit.Code}) {organizationUnit.DisplayName}";
    }

    protected virtual async Task<List<OrganizationUnitDto>> SearchOrganizationUnitsAsync(
        string? filter,
        int maxResultCount = DefaultMaxResultCount)
    {
        _cachedTree ??= await OrganizationUnitAppService.GetTreeAsync();
        var flatUnits = Flatten(_cachedTree).ToList();

        if (string.IsNullOrWhiteSpace(filter))
        {
            return flatUnits.Take(maxResultCount).ToList();
        }

        return flatUnits
            .Where(unit => unit.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || unit.Code.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(maxResultCount)
            .ToList();
    }

    protected virtual async Task<OrganizationUnitDto?> TryGetOrganizationUnitAsync(Guid organizationUnitId)
    {
        try
        {
            return await OrganizationUnitAppService.GetAsync(organizationUnitId);
        }
        catch
        {
            return null;
        }
    }

    protected void InvalidateOrganizationUnitCache()
    {
        _cachedTree = null;
    }

    private static IEnumerable<OrganizationUnitDto> Flatten(IEnumerable<OrganizationUnitDto> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;

            if (node.Children.Count > 0)
            {
                foreach (var child in Flatten(node.Children))
                {
                    yield return child;
                }
            }
        }
    }
}
