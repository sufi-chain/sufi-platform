using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Application.Services;

public interface ISufiCrudAppService<TEntityDto, in TKey>
    : ISufiCrudAppService<TEntityDto, TKey, PagedAndSortedResultRequestDto>
{
}

public interface ISufiCrudAppService<TEntityDto, in TKey, in TGetListInput>
    : ISufiCrudAppService<TEntityDto, TKey, TGetListInput, TEntityDto>
{
}

public interface ISufiCrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput>
    : ISufiCrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>
{
}

public interface ISufiCrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
    : IApplicationService
{
    Task<TEntityDto> GetAsync(TKey id);

    Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input);

    Task<TEntityDto> CreateAsync(TCreateInput input);

    Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input);

    Task DeleteAsync(TKey id);
}
