using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.ShortLinks;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class ShortUrlToShortUrlDtoMapper : MapperBase<ShortUrl, ShortUrlDto>
{
    [MapperIgnoreTarget(nameof(ShortUrlDto.FullShortUrl))]
    public override partial ShortUrlDto Map(ShortUrl source);
    [MapperIgnoreTarget(nameof(ShortUrlDto.FullShortUrl))]
    public override partial void Map(ShortUrl source, ShortUrlDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ShortUrlClickToShortUrlClickDtoMapper : MapperBase<ShortUrlClick, ShortUrlClickDto>
{
    public override partial ShortUrlClickDto Map(ShortUrlClick source);
    public override partial void Map(ShortUrlClick source, ShortUrlClickDto destination);
}

