namespace SufiChain.SufiAbp.TagsManagement.Settings;

/// <summary>
/// Loads Tags Management tenant policy from settings.
/// </summary>
public interface ITagsManagementPolicyProvider
{
    Task<TagsManagementPolicyDto> GetAsync(CancellationToken cancellationToken = default);
}
