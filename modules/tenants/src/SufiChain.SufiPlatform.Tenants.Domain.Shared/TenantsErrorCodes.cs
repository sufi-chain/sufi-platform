namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Business error codes of the tenants module (sufi-chain#441). Every code is localized in
/// Localization/Tenants/*.json, so the central error presenter shows the cause, never the generic internal error.
/// </summary>
public static class TenantsErrorCodes
{
    public const string Namespace = "TenantManagement";

    public const string DuplicateTenantName = Namespace + ":DuplicateTenantName";

    public const string DuplicateDatabaseName = Namespace + ":DuplicateDatabaseName";

    public const string DuplicatePrimarySubdomain = Namespace + ":DuplicatePrimarySubdomain";

    public const string DuplicateDomainHost = Namespace + ":DuplicateDomainHost";

    public const string DatabaseNameIsImmutable = Namespace + ":DatabaseNameIsImmutable";

    public const string UnverifiedCustomDomainCanNotBeActive = Namespace + ":UnverifiedCustomDomainCanNotBeActive";
}
