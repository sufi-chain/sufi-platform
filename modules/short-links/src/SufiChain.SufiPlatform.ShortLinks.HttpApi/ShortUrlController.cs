using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.ShortLinks;

[Area(ShortLinksRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ShortLinksRemoteServiceConsts.RemoteServiceName)]
[Route("api/short-link/short-urls")]
public class ShortUrlController : SufiControllerBase, IShortUrlAppService
{
    private readonly IShortUrlAppService _service;
    
    public ShortUrlController(IShortUrlAppService service)
    {
        _service = service;
    }
    
    [HttpPost]
    public Task<ShortUrlDto> CreateAsync(CreateShortUrlDto input)
        => _service.CreateAsync(input);
    
    [HttpPost("generate")]
    public Task<string> GenerateShortUrlAsync(CreateShortUrlDto input)
        => _service.GenerateShortUrlAsync(input);
    
    [HttpGet("{id}")]
    public Task<ShortUrlDto> GetAsync(Guid id)
        => _service.GetAsync(id);
    
    [HttpGet("by-code/{shortCode}")]
    public Task<ShortUrlDto> GetByShortCodeAsync(string shortCode)
        => _service.GetByShortCodeAsync(shortCode);
    
    [HttpGet]
    public Task<PagedResultDto<ShortUrlDto>> GetListAsync([FromQuery] GetShortUrlListDto input)
        => _service.GetListAsync(input);
    
    [HttpPut("{id}")]
    public Task<ShortUrlDto> UpdateAsync(Guid id, UpdateShortUrlDto input)
        => _service.UpdateAsync(id, input);
    
    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
        => _service.DeleteAsync(id);
    
    [HttpGet("{id}/analytics")]
    public Task<ShortUrlAnalyticsDto> GetAnalyticsAsync(Guid id)
        => _service.GetAnalyticsAsync(id);
}

