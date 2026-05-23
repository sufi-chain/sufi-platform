using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.Application.Services;

public interface ISufiAbpCrudAppService<TEntityDto, in TKey>
    : ISufiAbpCrudAppService<TEntityDto, TKey, PagedAndSortedResultRequestDto>
{
}

public interface ISufiAbpCrudAppService<TEntityDto, in TKey, in TGetListInput>
    : ISufiAbpCrudAppService<TEntityDto, TKey, TGetListInput, TEntityDto>
{
}

public interface ISufiAbpCrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput>
    : ISufiAbpCrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>
{
}

public interface ISufiAbpCrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
    : IApplicationService
{
    Task<TEntityDto> GetAsync(TKey id);

    Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input);

    Task<TEntityDto> CreateAsync(TCreateInput input);

    Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input);

    Task DeleteAsync(TKey id);
}
