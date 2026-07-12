namespace SufiChain.SufiPlatform.Tags.Settings;

/// <summary>
/// Resolved tenant policy for Tags Management.
/// </summary>
public class TagsPolicyDto
{
    public int MaxTagsPerEntity { get; set; } = 10;

    public int MaxTagNameLength { get; set; } = Tags.TagConsts.MaxNameLength;
}
