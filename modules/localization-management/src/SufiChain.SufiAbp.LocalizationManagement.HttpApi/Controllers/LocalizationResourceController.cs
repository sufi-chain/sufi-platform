using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.LocalizationManagement.Controllers;

[RemoteService(Name = LocalizationManagementRemoteServiceConsts.RemoteServiceName)]
[Area(LocalizationManagementRemoteServiceConsts.ModuleName)]
[Route("api/localization-management/resources")]
public class LocalizationResourceController : LocalizationManagementController, ILocalizationResourceAppService
{
    private readonly ILocalizationResourceAppService _service;

    public LocalizationResourceController(ILocalizationResourceAppService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public Task<LocalizationResourceDto> GetAsync(Guid id)
    {
        return _service.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<LocalizationResourceDto>> GetListAsync([FromQuery] GetLocalizationResourcesInput input)
    {
        return _service.GetListAsync(input);
    }

    [HttpGet("summary")]
    public Task<List<LocalizationResourceSummaryDto>> GetSummaryListAsync()
    {
        return _service.GetSummaryListAsync();
    }

    [HttpPost]
    public Task<LocalizationResourceDto> CreateAsync([FromBody] CreateUpdateLocalizationResourceDto input)
    {
        return _service.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public Task<LocalizationResourceDto> UpdateAsync(Guid id, [FromBody] CreateUpdateLocalizationResourceDto input)
    {
        return _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _service.DeleteAsync(id);
    }

    [HttpPut("{id}/enabled")]
    public Task SetEnabledAsync(Guid id, [FromQuery] bool isEnabled)
    {
        return _service.SetEnabledAsync(id, isEnabled);
    }
}
