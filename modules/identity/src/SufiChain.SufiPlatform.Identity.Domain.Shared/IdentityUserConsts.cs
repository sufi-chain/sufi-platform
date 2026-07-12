using SufiChain.SufiPlatform.Users;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Identity;

public static class IdentityUserConsts
{
    public static int MaxUserNameLength { get; set; } = SufiUserConsts.MaxUserNameLength;

    public static int MaxNameLength { get; set; } = SufiUserConsts.MaxNameLength;

    public static int MaxSurnameLength { get; set; } = SufiUserConsts.MaxSurnameLength;

    public static int MaxNormalizedUserNameLength { get; set; } = MaxUserNameLength;

    public static int MaxEmailLength { get; set; } = SufiUserConsts.MaxEmailLength;

    public static int MaxNormalizedEmailLength { get; set; } = MaxEmailLength;

    public static int MaxPhoneNumberLength { get; set; } = SufiUserConsts.MaxPhoneNumberLength;

    /// <summary>
    /// Default value: 128
    /// </summary>
    public static int MaxPasswordLength { get; set; } = 128;

    /// <summary>
    /// Default value: 256
    /// </summary>
    public static int MaxPasswordHashLength { get; set; } = 256;

    /// <summary>
    /// Default value: 256
    /// </summary>
    public static int MaxSecurityStampLength { get; set; } = 256;

    /// <summary>
    /// Default value: 16
    /// </summary>
    public static int MaxLoginProviderLength { get; set; } = 16;
}
