using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;
using Volo.Abp;

namespace SufiChain.SufiAbp.LocalizationManagement.Controllers;

[RemoteService(Name = LocalizationManagementRemoteServiceConsts.RemoteServiceName)]
[Area(LocalizationManagementRemoteServiceConsts.ModuleName)]
[Route("api/localization-management/business-editor")]
public class BusinessLocalizationEditorController : LocalizationManagementController, IBusinessLocalizationEditorAppService
{
    private readonly IBusinessLocalizationEditorAppService _service;

    public BusinessLocalizationEditorController(IBusinessLocalizationEditorAppService service)
    {
        _service = service;
    }

    [HttpPost("key-values")]
    public Task<BusinessLocalizationKeyValuesDto> GetKeyValuesAsync([FromBody] GetBusinessLocalizationKeyValuesInput input)
    {
        return _service.GetKeyValuesAsync(input);
    }

    [HttpPut("key-values")]
    public Task SaveKeyValuesAsync([FromBody] SaveBusinessLocalizationKeyValuesInput input)
    {
        return _service.SaveKeyValuesAsync(input);
    }

    [HttpGet("cultures")]
    public Task<List<BusinessLocalizationCultureDto>> GetEditorCulturesAsync()
    {
        return _service.GetEditorCulturesAsync();
    }
}
