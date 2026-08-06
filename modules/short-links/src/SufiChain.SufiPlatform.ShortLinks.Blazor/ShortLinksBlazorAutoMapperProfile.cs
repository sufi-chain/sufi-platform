using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.ShortLinks.Blazor;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ShortUrlDtoToCreateShortUrlDtoMapper : MapperBase<ShortUrlDto, CreateShortUrlDto>
{
    public override partial CreateShortUrlDto Map(ShortUrlDto source);
    public override partial void Map(ShortUrlDto source, CreateShortUrlDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ShortUrlDtoToUpdateShortUrlDtoMapper : MapperBase<ShortUrlDto, UpdateShortUrlDto>
{
    public override partial UpdateShortUrlDto Map(ShortUrlDto source);
    public override partial void Map(ShortUrlDto source, UpdateShortUrlDto destination);
}

