namespace SufiChain.SufiPlatform.Tenants;

public static class TenantConsts
{
    /// <summary>
    /// Default value: 64
    /// </summary>
    public static int MaxNameLength { get; set; } = 64;

    /// <summary>
    /// Default value: 128
    /// </summary>
    public static int MaxPasswordLength { get; set; } = 128;

    /// <summary>
    /// Default value: 256
    /// </summary>
    public static int MaxAdminEmailAddressLength { get; set; } = 256; 

    public const int MaxDatabaseNameLength = 128;

    public const int MaxSubdomainLength = 63;

    public const int MaxDomainHostLength = 253;
}
