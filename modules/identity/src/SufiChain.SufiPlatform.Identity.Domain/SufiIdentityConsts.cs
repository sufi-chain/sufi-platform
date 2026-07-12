namespace SufiChain.SufiPlatform.Identity;

public static class IdentityClaimConsts
{
    public const int MaxClaimTypeLength = 256;
    public const int MaxClaimValueLength = 1024;
}

public static class IdentityUserClaimConsts
{
    public const int MaxClaimTypeLength = 256;
    public const int MaxClaimValueLength = 1024;
}

public static class IdentityRoleClaimConsts
{
    public const int MaxClaimTypeLength = 256;
    public const int MaxClaimValueLength = 1024;
}

public static class IdentityUserLoginConsts
{
    public const int MaxLoginProviderLength = 128;
    public const int MaxProviderKeyLength = 128;
    public const int MaxProviderDisplayNameLength = 128;
}

public static class IdentityUserTokenConsts
{
    public const int MaxLoginProviderLength = 128;
    public const int MaxNameLength = 128;
}

public static class OrganizationUnitConsts
{
    public const int MaxDisplayNameLength = 128;
    public const int MaxCodeLength = 95;
    public const int CodeUnitLength = 5;
}

public static class IdentityClaimTypeConsts
{
    public const int MaxNameLength = 256;
    public const int MaxRegexLength = 512;
    public const int MaxRegexDescriptionLength = 128;
    public const int MaxDescriptionLength = 256;
}

public static class IdentitySessionConsts
{
    public const int MaxSessionIdLength = 128;
    public const int MaxDeviceLength = 64;
    public const int MaxDeviceInfoLength = 64;
    public const int MaxClientIdLength = 64;
    public const int MaxIpAddressesLength = 256;
}
