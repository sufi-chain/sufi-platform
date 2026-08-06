using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.FileManager.FileStructures;

public interface IFileStructureRepository : IRepository<FileStructure, Guid>
{
    Task<FileStructure?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> KeyExistsAsync(
        string key,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
