using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.AuditLogging;

public interface IAuditLogExcelFileRepository : IBasicRepository<AuditLogExcelFile, Guid>
{
    Task<List<AuditLogExcelFile>> GetListCreationTimeBeforeAsync(
        DateTime creationTimeBefore,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);
}
