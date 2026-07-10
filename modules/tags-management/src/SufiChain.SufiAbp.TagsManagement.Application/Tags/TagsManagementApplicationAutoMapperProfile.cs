using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class TagToTagDtoMapper : MapperBase<Tag, TagDto>
{
    public override partial TagDto Map(Tag source);
    public override partial void Map(Tag source, TagDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class TagLinkToTagLinkDtoMapper : MapperBase<TagLink, TagLinkDto>
{
    public override partial TagLinkDto Map(TagLink source);
    public override partial void Map(TagLink source, TagLinkDto destination);
}
