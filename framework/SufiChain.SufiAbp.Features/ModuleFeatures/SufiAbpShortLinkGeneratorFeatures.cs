namespace SufiChain.SufiAbp.ShortLinkGenerator.Features;

/// <summary>
/// Shared feature names for SufiAbp Short Link Generator capabilities.
/// </summary>
public static class SufiAbpShortLinkGeneratorFeatures
{
    public const string GroupName = "ShortLinkGenerator";

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
