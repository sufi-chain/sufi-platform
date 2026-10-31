using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Tags.Settings;

public class TagsSettingsDto
{
    public int MaxTagsPerEntity { get; set; }
    public int MaxTagNameLength { get; set; }
}

public interface ITagsSettingsAppService : IApplicationService
{
    Task<TagsSettingsDto> GetAsync();
    Task UpdateAsync(TagsSettingsDto input);
}
