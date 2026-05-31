namespace SufiChain.SufiAbp.Data;


public static class SufiAbpConstants
{
    /// <summary>
    /// Default email for the seeded admin user.
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
    /// Default sender email address for outbound mail.
    /// </summary>
    public const string DefaultFromAddress = "noreply@sufichain.ir";

    /// <summary>
    /// Default sender display name for outbound mail.
    /// </summary>
    public const string DefaultFromDisplayName = "Sufi Platform";
}
