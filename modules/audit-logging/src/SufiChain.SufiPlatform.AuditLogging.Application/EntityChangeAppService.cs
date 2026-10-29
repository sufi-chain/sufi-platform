using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using SufiChain.SufiPlatform.AuditLogging.Permissions;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// Application service for managing entity changes.
/// </summary>
[Authorize(AuditLoggingPermissions.EntityChanges.Default)]
public class EntityChangeAppService : SufiApplicationService, IEntityChangeAppService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public EntityChangeAppService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public virtual async Task<PagedResultDto<EntityChangeListItemDto>> GetListAsync(GetEntityChangeListInput input)
    {
        var userId = await RestrictedUserIdAsync();
        var totalCount = await _auditLogRepository.GetEntityChangeCountAsync(
            auditLogId: input.AuditLogId,
            startTime: input.StartTime,
            endTime: input.EndTime,
            changeType: input.ChangeType,
            entityId: input.EntityId,
            entityTypeFullName: input.EntityTypeFullName,
            userId: userId
        );

        var entityChanges = await _auditLogRepository.GetEntityChangeListAsync(
            sorting: input.Sorting ?? "ChangeTime DESC",
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            auditLogId: input.AuditLogId,
            startTime: input.StartTime,
            endTime: input.EndTime,
            changeType: input.ChangeType,
            entityId: input.EntityId,
            entityTypeFullName: input.EntityTypeFullName,
            includeDetails: input.IncludeDetails,
            userId: userId
        );

        // Get usernames for each entity change
        var items = new List<EntityChangeListItemDto>();
        foreach (var entityChange in entityChanges)
        {
            var withUsername = await _auditLogRepository.GetEntityChangeWithUsernameAsync(entityChange.Id);
            items.Add(ObjectMapper.Map<EntityChangeWithUsername, EntityChangeListItemDto>(withUsername));
        }

        return new PagedResultDto<EntityChangeListItemDto>(totalCount, items);
    }

    public virtual async Task<EntityChangeDto> GetAsync(Guid id)
    {
        var entityChange = await _auditLogRepository.GetEntityChange(id);
        var auditLog = await _auditLogRepository.FindAsync(entityChange.AuditLogId);
        if (auditLog == null || !AuditLogViewerScope.CanReadEntry(await CanViewOtherUsersAsync(), CurrentUser.Id, auditLog.UserId))
        {
            throw new EntityNotFoundException(typeof(EntityChange), id);
        }

        return ObjectMapper.Map<EntityChange, EntityChangeDto>(entityChange);
    }

    public virtual async Task<List<EntityChangeListItemDto>> GetEntityChangesAsync(
        string entityId, 
        string entityTypeFullName)
    {
        var entityChanges = await _auditLogRepository.GetEntityChangesWithUsernameAsync(
            entityId,
            entityTypeFullName,
            await RestrictedUserIdAsync());
        return ObjectMapper.Map<List<EntityChangeWithUsername>, List<EntityChangeListItemDto>>(entityChanges);
    }

    protected virtual async Task<Guid?> RestrictedUserIdAsync()
    {
        if (await CanViewOtherUsersAsync())
        {
            return null;
        }

        return AuditLogViewerScope.EffectiveUserId(false, CurrentUser.Id, null);
    }

    protected virtual async Task<bool> CanViewOtherUsersAsync()
    {
        return AuditLogViewerScope.CanViewOtherUsers(
            CurrentUser.IsInRole(AuditLogViewerScope.AdminRoleName),
            await AuthorizationService.IsGrantedAsync(AuditLoggingPermissions.AuditLogs.ViewAll));
    }
}
