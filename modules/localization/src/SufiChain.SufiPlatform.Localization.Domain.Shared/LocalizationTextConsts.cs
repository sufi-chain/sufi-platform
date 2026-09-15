namespace SufiChain.SufiPlatform.Localization;

/// <summary>
/// Length limits for <c>LocalizationText</c>.
/// <see cref="MaxValueLength"/> matches copilot system-prompt capacity because
/// seeded copilots store prompts as business localization values.
/// </summary>
public static class LocalizationTextConsts
{
    public const int MaxResourceNameLength = 128;
    public const int MaxCultureNameLength = 16;
    public const int MaxKeyLength = 512;

    /// <summary>
    /// Must stay at least <c>CopilotConsts.MaxSystemPromptLength</c> (16000).
    /// The SQL column is already <c>nvarchar(max)</c>; this constant is the application cap.
    /// </summary>
    public const int MaxValueLength = 16000;
}
