using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Editions.Controllers;

[Area(EditionsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = EditionsRemoteServiceConsts.RemoteServiceName)]
[Route("api/editions/editions")]
public class EditionController : SufiControllerBase, IEditionAppService
{
    private readonly IEditionAppService _editionAppService;

    public EditionController(IEditionAppService editionAppService)
    {
        _editionAppService = editionAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<EditionDto> GetAsync(Guid id) => _editionAppService.GetAsync(id);

    [HttpGet]
    public virtual Task<PagedResultDto<EditionDto>> GetListAsync(GetEditionsInput input) =>
        _editionAppService.GetListAsync(input);

    [HttpPost]
    public virtual Task<EditionDto> CreateAsync(EditionCreateDto input) =>
        _editionAppService.CreateAsync(input);

    [HttpPut]
    [Route("{id}")]
    public virtual Task<EditionDto> UpdateAsync(Guid id, EditionUpdateDto input) =>
        _editionAppService.UpdateAsync(id, input);

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id) => _editionAppService.DeleteAsync(id);
}
