using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.PermissionManagement;

public interface IPermissionDefinitionRecordRepository : IBasicRepository<PermissionDefinitionRecord, Guid>
{
    Task<PermissionDefinitionRecord> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}