using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public interface IFileManagerStoragePolicyProvider
{
    Task<FileManagerStoragePolicy> GetAsync(CancellationToken cancellationToken = default);
}
