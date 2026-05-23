using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.AuditLogging.Dtos;
using SufiChain.SufiAbp.AuditLogging;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.AuditLogging;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AuditLogToAuditLogDtoMapper : MapperBase<AuditLog, AuditLogDto>
{
    public override partial AuditLogDto Map(AuditLog source);
    public override partial void Map(AuditLog source, AuditLogDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AuditLogToAuditLogListItemDtoMapper : MapperBase<AuditLog, AuditLogListItemDto>
{
    [MapperIgnoreTarget(nameof(AuditLogListItemDto.HasException))]
    public override partial AuditLogListItemDto Map(AuditLog source);

    [MapperIgnoreTarget(nameof(AuditLogListItemDto.HasException))]
    public override partial void Map(AuditLog source, AuditLogListItemDto destination);

    public override void AfterMap(AuditLog source, AuditLogListItemDto destination)
    {
        destination.HasException = !string.IsNullOrEmpty(source.Exceptions);
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class AuditLogActionToAuditLogActionDtoMapper : MapperBase<AuditLogAction, AuditLogActionDto>
{
    public override partial AuditLogActionDto Map(AuditLogAction source);
    public override partial void Map(AuditLogAction source, AuditLogActionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class EntityChangeToEntityChangeDtoMapper : MapperBase<EntityChange, EntityChangeDto>
{
    public override partial EntityChangeDto Map(EntityChange source);
    public override partial void Map(EntityChange source, EntityChangeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class EntityChangeToEntityChangeListItemDtoMapper : MapperBase<EntityChange, EntityChangeListItemDto>
{
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.UserName))]
    public override partial EntityChangeListItemDto Map(EntityChange source);
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.UserName))]
    public override partial void Map(EntityChange source, EntityChangeListItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class EntityChangeWithUsernameToEntityChangeListItemDtoMapper : MapperBase<EntityChangeWithUsername, EntityChangeListItemDto>
{
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.Id))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.AuditLogId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.TenantId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.ChangeTime))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.ChangeType))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.EntityTypeFullName))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.EntityId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.PropertyChanges))]
    public override partial EntityChangeListItemDto Map(EntityChangeWithUsername source);

    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.Id))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.AuditLogId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.TenantId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.ChangeTime))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.ChangeType))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.EntityTypeFullName))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.EntityId))]
    [MapperIgnoreTarget(nameof(EntityChangeListItemDto.PropertyChanges))]
    public override partial void Map(EntityChangeWithUsername source, EntityChangeListItemDto destination);

    public override void AfterMap(EntityChangeWithUsername source, EntityChangeListItemDto destination)
    {
        destination.Id = source.EntityChange.Id;
        destination.AuditLogId = source.EntityChange.AuditLogId;
        destination.TenantId = source.EntityChange.TenantId;
        destination.ChangeTime = source.EntityChange.ChangeTime;
        destination.ChangeType = source.EntityChange.ChangeType;
        destination.EntityTypeFullName = source.EntityChange.EntityTypeFullName;
        destination.EntityId = source.EntityChange.EntityId;
        destination.PropertyChanges = source.EntityChange.PropertyChanges?
            .Select(p => new EntityPropertyChangeDto
            {
                Id = p.Id,
                TenantId = p.TenantId,
                EntityChangeId = p.EntityChangeId,
                PropertyName = p.PropertyName,
                PropertyTypeFullName = p.PropertyTypeFullName,
                OriginalValue = p.OriginalValue,
                NewValue = p.NewValue
            }).ToList() ?? new();
        destination.UserName = source.UserName;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class EntityPropertyChangeToEntityPropertyChangeDtoMapper : MapperBase<EntityPropertyChange, EntityPropertyChangeDto>
{
    public override partial EntityPropertyChangeDto Map(EntityPropertyChange source);
    public override partial void Map(EntityPropertyChange source, EntityPropertyChangeDto destination);
}
