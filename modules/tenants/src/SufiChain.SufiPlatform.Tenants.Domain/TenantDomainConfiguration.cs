namespace SufiChain.SufiPlatform.Tenants;

public sealed record TenantDomainConfiguration(
    string Host,
    TenantDomainType Type,
    bool IsVerified,
    bool IsActive);
