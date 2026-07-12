namespace SufiChain.SufiPlatform.ShortLinks.Features;

/// <summary>
/// Shared feature names for Sufi Short Link Generator capabilities.
/// </summary>
public static class SufiShortLinksFeatures
{
    public const string GroupName = "SufiShortLinks";

    /// <summary>
    /// Master switch for the Short Link Generator module.
    /// </summary>
    public const string Enable = GroupName + ".Enable";

    /// <summary>
    /// Short URL creation, management, and administration.
    /// </summary>
    public const string ShortLinks = GroupName + ".ShortLinks";

    /// <summary>
    /// Click tracking and analytics.
    /// </summary>
    public const string Analytics = GroupName + ".Analytics";

    /// <summary>
    /// Public short URL redirect resolution.
    /// </summary>
    public const string PublicRedirect = GroupName + ".PublicRedirect";
}