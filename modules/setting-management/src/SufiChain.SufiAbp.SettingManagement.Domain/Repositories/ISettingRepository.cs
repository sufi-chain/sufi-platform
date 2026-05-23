using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.SettingManagement;

public interface ISettingRepository : IBasicRepository<Setting, Guid>
{
    Task<Setting> FindAsync(
        string name,
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    Task<List<Setting>> GetListAsync(
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    Task<List<Setting>> GetListAsync(
        string[] names,
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);
}
