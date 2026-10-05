namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Anonymous menu reads are limited to contexts that are publicly visible.
/// The public site landing menu (<c>Public</c>) and CMS menu blocks (<c>SufiCMS</c>) stay readable.
/// Any other context is readable only when a module policy allows that exact context.
/// </summary>
public class PublicMenuAnonymousAccess
{
    public const string PublicSiteContextType = "Public";
    public const string CmsContextType = "SufiCMS";

    private readonly IEnumerable<IPublicMenuContextVisibility> _policies;

    public PublicMenuAnonymousAccess(IEnumerable<IPublicMenuContextVisibility>? policies)
    {
        _policies = policies ?? [];
    }

    public virtual async Task<bool> AllowsAsync(string? contextType, Guid? contextId)
    {
        if (string.IsNullOrWhiteSpace(contextType))
        {
            return false;
        }

        var allowedByPolicy = false;
        foreach (var policy in _policies)
        {
            if (policy == null)
            {
                continue;
            }

            var decision = await policy.IsPubliclyVisibleAsync(contextType, contextId);
            if (decision == false)
            {
                return false;
            }

            if (decision == true)
            {
                allowedByPolicy = true;
            }
        }

        return allowedByPolicy || IsBuiltInPublicContext(contextType);
    }

    public static bool IsBuiltInPublicContext(string? contextType)
    {
        return string.Equals(contextType, PublicSiteContextType, StringComparison.OrdinalIgnoreCase)
            || string.Equals(contextType, CmsContextType, StringComparison.OrdinalIgnoreCase);
    }
}
