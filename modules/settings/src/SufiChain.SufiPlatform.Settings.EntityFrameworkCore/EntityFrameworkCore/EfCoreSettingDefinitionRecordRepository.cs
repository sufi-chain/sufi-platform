using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Settings.EntityFrameworkCore;

public class EfCoreSettingDefinitionRecordRepository : EfCoreRepository<ISettingsDbContext, SettingDefinitionRecord, Guid>, ISettingDefinitionRecordRepository
{
    public EfCoreSettingDefinitionRecordRepository(IDbContextProvider<ISettingsDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<SettingDefinitionRecord> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }
}
