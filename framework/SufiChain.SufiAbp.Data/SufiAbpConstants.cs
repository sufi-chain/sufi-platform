namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Sufi Platform overrides for ABP framework constants. Uses sufichain.ir domain instead of abp.io.
/// Use these constants when passing seed context properties (e.g. in DbMigrationService) or when
/// configuring email settings. Replaces ABP defaults: admin@sabp.com, noreply@abp.io, "ABP application".
/// </summary>
public static class SufiAbpConstants
{
    /// <summary>
    /// Default email for the seeded admin user (overrides ABP's admin@sabp.com).
    /// </summary>
    public const string AdminEmailDefaultValue = "admin@sufichain.ir";

    /// <summary>
    /// Property name for admin email in DataSeedContext (same as <see cref="SufiChain.SufiAbp.Identity.IdentityDataSeedContributor.AdminEmailPropertyName"/>).
    /// </summary>
    public const string AdminEmailPropertyName = "AdminEmail";

    /// <summary>
    /// Property name for admin password in DataSeedContext (same as IdentityDataSeedContributor.AdminPasswordPropertyName).
    /// </summary>
    public const string AdminPasswordPropertyName = "AdminPassword";

    /// <summary>
    /// Default admin password for seeded user (same as ABP's "1q2w3E*").
    /// </summary>
    public const string AdminPasswordDefaultValue = "1q2w3E*";

    /// <summary>
    /// Default sender email address for outbound mail (overrides ABP's noreply@abp.io).
    /// </summary>
    public const string DefaultFromAddress = "noreply@sufichain.ir";

    /// <summary>
    /// Default sender display name for outbound mail (overrides ABP's "ABP application").
    /// </summary>
    public const string DefaultFromDisplayName = "Sufi Platform";
}
