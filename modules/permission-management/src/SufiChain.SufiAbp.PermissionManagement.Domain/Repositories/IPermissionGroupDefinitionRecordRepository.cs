using System;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.PermissionManagement;

public interface IPermissionGroupDefinitionRecordRepository : IBasicRepository<PermissionGroupDefinitionRecord, Guid>
{
    
}