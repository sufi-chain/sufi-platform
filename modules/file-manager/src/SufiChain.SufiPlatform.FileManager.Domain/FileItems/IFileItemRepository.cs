using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

public interface IFileItemRepository : IRepository<FileItem, Guid>
{
    Task<FileItem> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FileItem?> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IQueryable<FileItem>> GetQueryableAsync(CancellationToken cancellationToken = default);
    Task<FileItem> InsertAsync(FileItem entity, bool autoSave = false, CancellationToken cancellationToken = default);
    Task<FileItem> UpdateAsync(FileItem entity, bool autoSave = false, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default);

    Task<List<FileItem>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    Task<List<FileItem>> GetByStructureKeyAsync(
        string structureKey,
        CancellationToken cancellationToken = default);

    Task<List<FileItem>> GetTempFilesAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    Task<long> GetTotalSizeByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<FileItem?> FindByBlobNameAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    Task<List<FileItem>> SearchAsync(
        string? keyword = null,
        FileType? fileType = null,
        string? entityType = null,
        Guid? entityId = null,
        string? structureKey = null,
        bool? onlyFromPublicStructures = null,
        int skipCount = 0,
        int maxResultCount = 10,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? keyword = null,
        FileType? fileType = null,
        string? entityType = null,
        Guid? entityId = null,
        string? structureKey = null,
        bool? onlyFromPublicStructures = null,
        CancellationToken cancellationToken = default);
}
