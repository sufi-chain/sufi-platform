using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public interface IFileStorageQuotaGuard
{
    Task ExecuteAsync(
        long positiveByteDelta,
        Func<Task> action,
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteAsync<TResult>(
        long positiveByteDelta,
        Func<Task<TResult>> action,
        CancellationToken cancellationToken = default);
}
