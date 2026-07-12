namespace SufiChain.SufiPlatform.Settings;

public static class SettingConsts
{
    /// <summary>
    /// Maximum length for setting value.
    /// Default: 2048
    /// </summary>
    public static int MaxValueLengthValue { get; set; } = 2048;

    /// <summary>
    /// Maximum length for setting name.
    /// Default: 512
    /// </summary>
    public static int MaxNameLength { get; set; } = 512;

    /// <summary>
    /// Maximum length for provider name.
    /// Default: 64
    /// </summary>
    public static int MaxProviderNameLength { get; set; } = 64;

    /// <summary>
    /// Maximum length for provider key.
    /// Default: 64
    /// </summary>
    public static int MaxProviderKeyLength { get; set; } = 64;
}
