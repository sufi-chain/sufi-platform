namespace SufiChain.SufiPlatform.Tags.Settings;

/// <summary>
/// Loads Tags Management tenant policy from settings.
/// </summary>
public interface ITagsPolicyProvider
{
    Task<TagsPolicyDto> GetAsync(CancellationToken cancellationToken = default);
}
