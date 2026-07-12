using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver.Linq;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

public class MongoSettingDefinitionRecordRepository : MongoDbRepository<ISettingsMongoDbContext, SettingDefinitionRecord, Guid>, ISettingDefinitionRecordRepository
{
    public MongoSettingDefinitionRecordRepository(IMongoDbContextProvider<ISettingsMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<SettingDefinitionRecord> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(s => s.Name == name, GetCancellationToken(cancellationToken));
    }
}
