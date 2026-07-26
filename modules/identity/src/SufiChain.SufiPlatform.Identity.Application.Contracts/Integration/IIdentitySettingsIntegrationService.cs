using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Identity.Integration;

/// <summary>
/// Integration service for Identity settings consumed by other modules (e.g. password policy).
/// </summary>
[IntegrationService]
public interface IIdentitySettingsIntegrationService : IApplicationService
{
    /// <summary>
    /// Returns the current password policy requirements from Identity settings.
    /// </summary>
    Task<IdentityPasswordRequirementsDto> GetPasswordRequirementsAsync();
}

/// <summary>
/// Password policy requirements exposed across module boundaries.
/// </summary>
public class IdentityPasswordRequirementsDto
{
    /// <summary>Minimum password length.</summary>
    public int RequiredLength { get; set; }

    /// <summary>Whether a digit is required.</summary>
    public bool RequireDigit { get; set; }

    /// <summary>Whether a lowercase letter is required.</summary>
    public bool RequireLowercase { get; set; }

    /// <summary>Whether an uppercase letter is required.</summary>
    public bool RequireUppercase { get; set; }

    /// <summary>Whether a non-alphanumeric character is required.</summary>
    public bool RequireNonAlphanumeric { get; set; }
}
