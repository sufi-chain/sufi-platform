namespace SufiChain.SufiPlatform.UI.Blazor.Timing;

/// <summary>
/// Reads <c>Abp.Timing.TimeZone</c> and converts persisted UTC instants for display.
/// Storage stays on the ABP clock. This type does not change stored values.
/// </summary>
public interface IUserTimeZoneDisplay
{
    /// <summary>
    /// Resolves the current tenant (or host) time zone. An unset setting is Tehran.
    /// </summary>
    Task<TimeZoneInfo> GetAsync();
}
