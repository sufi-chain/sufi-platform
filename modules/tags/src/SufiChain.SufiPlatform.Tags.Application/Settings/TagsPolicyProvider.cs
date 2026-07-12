using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Tags.Settings;

/// <inheritdoc />
public class TagsPolicyProvider : ITagsPolicyProvider, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public TagsPolicyProvider(ISettingProvider settingProvider)
    {
        SettingProvider = settingProvider;
    }

    public virtual async Task<TagsPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var maxTags = await SettingProvider.GetAsync<int>(TagsSettings.MaxTagsPerEntity);
        if (maxTags <= 0)
        {
            maxTags = 10;
        }

        var maxNameLength = await SettingProvider.GetAsync<int>(TagsSettings.MaxTagNameLength);
        if (maxNameLength <= 0)
        {
            maxNameLength = Tags.TagConsts.MaxNameLength;
        }
        else if (maxNameLength > Tags.TagConsts.MaxNameLength)
        {
            maxNameLength = Tags.TagConsts.MaxNameLength;
        }

        return new TagsPolicyDto
        {
            MaxTagsPerEntity = maxTags,
            MaxTagNameLength = maxNameLength
        };
    }
}
