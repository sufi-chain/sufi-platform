using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public interface IShortUrlAppService : IApplicationService
{
    Task<ShortUrlDto> CreateAsync(CreateShortUrlDto input);
    
    Task<ShortUrlDto> GetAsync(Guid id);
    
    Task<ShortUrlDto> GetByShortCodeAsync(string shortCode);
    
    Task<PagedResultDto<ShortUrlDto>> GetListAsync(GetShortUrlListDto input);
    
    Task<ShortUrlDto> UpdateAsync(Guid id, UpdateShortUrlDto input);
    
    Task DeleteAsync(Guid id);
    
    Task<ShortUrlAnalyticsDto> GetAnalyticsAsync(Guid id);
    
    Task<string> GenerateShortUrlAsync(CreateShortUrlDto input);
}

