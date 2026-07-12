using System.Security.Claims;
using SufiChain.SufiPlatform.UI.Users;

namespace SufiChain.SufiPlatform.UI.Services.Users;

/// <summary>
/// Default anonymous current-user accessor for hosts that do not provide an authentication bridge.
/// </summary>
public class DefaultCurrentUserAccessor : ICurrentUserAccessor
{
    public bool IsAuthenticated => false;

    public Guid? Id => null;

    public string? UserName => null;

    public string? Name => null;

    public string? SurName => null;

    public string? PhoneNumber => null;

    public bool PhoneNumberVerified => false;

    public string? Email => null;

    public bool EmailVerified => false;

    public Guid? TenantId => null;

    public string[] Roles => Array.Empty<string>();

    public Claim? FindClaim(string claimType)
    {
        return null;
    }

    public Claim[] FindClaims(string claimType)
    {
        return Array.Empty<Claim>();
    }

    public Claim[] GetAllClaims()
    {
        return Array.Empty<Claim>();
    }

    public bool IsInRole(string roleName)
    {
        return false;
    }
}
