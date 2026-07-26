namespace SufiChain.SufiPlatform.Permissions;

/// <summary>
/// Error codes for the Permissions module.
/// </summary>
public static class PermissionsErrorCodes
{
    public const string PermissionDisabled = "Permissions:PermissionDisabled";
    public const string ProviderIncompatible = "Permissions:ProviderIncompatible";
    public const string MultiTenancySideIncompatible = "Permissions:MultiTenancySideIncompatible";
    public const string UnknownProvider = "Permissions:UnknownProvider";
    public const string InvalidPermissionName = "Permissions:InvalidPermissionName";
    public const string UnknownProviderKeyLookupService = "Permissions:UnknownProviderKeyLookupService";
    public const string ProviderKeyLookupServiceUnavailable = "Permissions:ProviderKeyLookupServiceUnavailable";
    public const string ResourcePermissionProviderUnavailable = "Permissions:ResourcePermissionProviderUnavailable";
}
