using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.TagsManagement.Settings;

/// <inheritdoc />
public class TagsManagementPolicyProvider : ITagsManagementPolicyProvider, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public TagsManagementPolicyProvider(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<TagsManagementPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var maxTags = await SettingProvider.GetAsync<int>(TagsManagementSettings.MaxTagsPerEntity);
        if (maxTags <= 0)
        {
            maxTags = 10;
        }

        var maxNameLength = await SettingProvider.GetAsync<int>(TagsManagementSettings.MaxTagNameLength);
        if (maxNameLength <= 0)
        {
            maxNameLength = Tags.TagConsts.MaxNameLength;
        }
        else if (maxNameLength > Tags.TagConsts.MaxNameLength)
        {
            maxNameLength = Tags.TagConsts.MaxNameLength;
        }

        return new TagsManagementPolicyDto
        {
            MaxTagsPerEntity = maxTags,
            MaxTagNameLength = maxNameLength
        };
    }
}
