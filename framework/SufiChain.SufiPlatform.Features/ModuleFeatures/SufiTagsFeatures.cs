namespace SufiChain.SufiPlatform.Tags.Features;

/// <summary>
/// Shared feature names for Sufi Tags Management capabilities.
/// </summary>
public static class SufiTagsFeatures
{
    public const string GroupName = "SufiTags";

    /// <summary>
    /// Master switch for the Tags Management module.
    /// </summary>
    public const string Enable = GroupName + ".Enable";

    /// <summary>
    /// Tag definition and administration.
    /// </summary>
    public const string Tags = GroupName + ".Tags";

    /// <summary>
    /// Assigning tags to entities.
    /// </summary>
    public const string TagLinks = GroupName + ".TagLinks";
}