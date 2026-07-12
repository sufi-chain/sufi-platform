using System;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Permissions;

public interface IPermissionGroupDefinitionRecordRepository : IBasicRepository<PermissionGroupDefinitionRecord, Guid>
{
    
}