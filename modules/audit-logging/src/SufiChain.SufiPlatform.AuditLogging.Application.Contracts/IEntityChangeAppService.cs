using SufiChain.SufiPlatform.AuditLogging.Dtos;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// Application service for managing entity changes.
/// </summary>
[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
public interface IEntityChangeAppService : IApplicationService
{
    /// <summary>
    /// Gets a paged list of entity changes.
    /// </summary>
    Task<PagedResultDto<EntityChangeListItemDto>> GetListAsync(GetEntityChangeListInput input);

    /// <summary>
    /// Gets a specific entity change by ID.
    /// </summary>
    Task<EntityChangeDto> GetAsync(Guid id);

    /// <summary>
    /// Gets entity changes with username for a specific entity.
    /// </summary>
    Task<List<EntityChangeListItemDto>> GetEntityChangesAsync(string entityId, string entityTypeFullName);
}
