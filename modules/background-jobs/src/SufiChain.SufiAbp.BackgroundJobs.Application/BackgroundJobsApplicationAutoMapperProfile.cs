using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.BackgroundJobs;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BackgroundJobRecordToBackgroundJobDtoMapper : MapperBase<BackgroundJobRecord, BackgroundJobDto>
{
    public override partial BackgroundJobDto Map(BackgroundJobRecord source);
    public override partial void Map(BackgroundJobRecord source, BackgroundJobDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class BackgroundJobRecordToBackgroundJobListItemDtoMapper : MapperBase<BackgroundJobRecord, BackgroundJobListItemDto>
{
    public override partial BackgroundJobListItemDto Map(BackgroundJobRecord source);
    public override partial void Map(BackgroundJobRecord source, BackgroundJobListItemDto destination);
}
