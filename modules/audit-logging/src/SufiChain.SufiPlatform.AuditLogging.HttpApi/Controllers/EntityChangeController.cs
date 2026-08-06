using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.AuditLogging.Controllers;

/// <summary>
/// Controller for entity change operations.
/// </summary>
[Area(AuditLoggingRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
[Route("api/audit-logging/entity-changes")]
public class EntityChangeController : SufiControllerBase, IEntityChangeAppService
{
    private readonly IEntityChangeAppService _entityChangeAppService;

    public EntityChangeController(IEntityChangeAppService entityChangeAppService)
    {
        _entityChangeAppService = entityChangeAppService;
    }

    /// <summary>
    /// Gets a paged list of entity changes.
    /// </summary>
    [HttpGet]
    public virtual Task<PagedResultDto<EntityChangeListItemDto>> GetListAsync([FromQuery] GetEntityChangeListInput input)
    {
        return _entityChangeAppService.GetListAsync(input);
    }

    /// <summary>
    /// Gets a specific entity change by ID.
    /// </summary>
    [HttpGet]
    [Route("{id}")]
    public virtual Task<EntityChangeDto> GetAsync(Guid id)
    {
        return _entityChangeAppService.GetAsync(id);
    }

    /// <summary>
    /// Gets entity changes for a specific entity.
    /// </summary>
    [HttpGet]
    [Route("by-entity")]
    public virtual Task<List<EntityChangeListItemDto>> GetEntityChangesAsync(
        [FromQuery] string entityId, 
        [FromQuery] string entityTypeFullName)
    {
        return _entityChangeAppService.GetEntityChangesAsync(entityId, entityTypeFullName);
    }
}
