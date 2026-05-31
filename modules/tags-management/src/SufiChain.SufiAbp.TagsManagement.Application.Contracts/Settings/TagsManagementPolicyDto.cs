namespace SufiChain.SufiAbp.TagsManagement.Settings;

/// <summary>
/// Resolved tenant policy for Tags Management.
/// </summary>
public class TagsManagementPolicyDto
{
    public int MaxTagsPerEntity { get; set; } = 10;

    public int MaxTagNameLength { get; set; } = Tags.TagConsts.MaxNameLength;
}
