using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.Localization.Dtos;
using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.Localization;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LocalizationTextToLocalizationTextDtoMapper : MapperBase<LocalizationText, LocalizationTextDto>
{
    public override partial LocalizationTextDto Map(LocalizationText source);
    public override partial void Map(LocalizationText source, LocalizationTextDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LocalizationTextToLocalizationTextWithBaseValueDtoMapper : MapperBase<LocalizationText, LocalizationTextWithBaseValueDto>
{
    [MapperIgnoreTarget(nameof(LocalizationTextWithBaseValueDto.BaseValue))]
    public override partial LocalizationTextWithBaseValueDto Map(LocalizationText source);
    [MapperIgnoreTarget(nameof(LocalizationTextWithBaseValueDto.BaseValue))]
    public override partial void Map(LocalizationText source, LocalizationTextWithBaseValueDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class LocalizationResourceToLocalizationResourceDtoMapper : MapperBase<LocalizationResource, LocalizationResourceDto>
{
    public override partial LocalizationResourceDto Map(LocalizationResource source);
    public override partial void Map(LocalizationResource source, LocalizationResourceDto destination);
}
