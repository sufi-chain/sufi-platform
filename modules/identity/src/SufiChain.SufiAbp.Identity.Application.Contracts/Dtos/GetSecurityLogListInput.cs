using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity.Dtos;

/// <summary>
/// Input DTO for getting a paged list of security logs.
/// </summary>
public class GetSecurityLogListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Filter by start time (inclusive).
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Filter by end time (inclusive - logs up to end of day are included).
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Filter by application name.
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Filter by identity type (e.g., "Identity", "OpenIddict", "IdentityExternal", "IdentityTwoFactor").
    /// </summary>
    public string? Identity { get; set; }

    /// <summary>
    /// Filter by action (e.g., "LoginSucceeded", "LoginFailed", "Logout").
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Filter by user ID.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Filter by username (partial match).
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Filter by OAuth client ID.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Filter by correlation ID.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Filter by client IP address.
    /// </summary>
    public string? ClientIpAddress { get; set; }
}
