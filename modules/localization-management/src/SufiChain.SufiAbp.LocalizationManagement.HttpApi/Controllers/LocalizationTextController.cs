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
[Route("api/sabp/localization-management/texts")]
public class LocalizationTextController : LocalizationManagementController, ILocalizationTextAppService
{
    private readonly ILocalizationTextAppService _service;

    public LocalizationTextController(ILocalizationTextAppService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public Task<LocalizationTextDto> GetAsync(Guid id)
    {
        return _service.GetAsync(id);
    }

    [HttpGet]
    public Task<PagedResultDto<LocalizationTextDto>> GetListAsync([FromQuery] GetLocalizationTextsInput input)
    {
        return _service.GetListAsync(input);
    }

    [HttpGet("all/{resourceName}/{cultureName}")]
    public Task<List<LocalizationTextWithBaseValueDto>> GetAllForResourceAsync(string resourceName, string cultureName)
    {
        return _service.GetAllForResourceAsync(resourceName, cultureName);
    }

    [HttpGet("merged")]
    public Task<PagedResultDto<LocalizationTextWithBaseValueDto>> GetMergedListAsync([FromQuery] GetMergedLocalizationTextsInput input)
    {
        return _service.GetMergedListAsync(input);
    }

    [HttpPost]
    public Task<LocalizationTextDto> CreateOrUpdateAsync([FromBody] CreateUpdateLocalizationTextDto input)
    {
        return _service.CreateOrUpdateAsync(input);
    }

    [HttpPut("{id}/value")]
    public Task<LocalizationTextDto> UpdateValueAsync(Guid id, [FromBody] UpdateLocalizationTextValueDto input)
    {
        return _service.UpdateValueAsync(id, input);
    }

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _service.DeleteAsync(id);
    }

    [HttpDelete("by-key")]
    public Task DeleteByKeyAsync([FromQuery] string resourceName, [FromQuery] string cultureName, [FromQuery] string key)
    {
        return _service.DeleteByKeyAsync(resourceName, cultureName, key);
    }

    [HttpGet("resource-names")]
    public Task<List<string>> GetResourceNamesAsync()
    {
        return _service.GetResourceNamesAsync();
    }

    [HttpGet("culture-names")]
    public Task<List<string>> GetCultureNamesAsync([FromQuery] string? resourceName = null)
    {
        return _service.GetCultureNamesAsync(resourceName);
    }

    [HttpPost("import")]
    public Task<ImportResultDto> ImportAsync([FromBody] ImportLocalizationTextsDto input)
    {
        return _service.ImportAsync(input);
    }

    [HttpPost("export")]
    public Task<string> ExportAsync([FromBody] ExportLocalizationTextsDto input)
    {
        return _service.ExportAsync(input);
    }
}
