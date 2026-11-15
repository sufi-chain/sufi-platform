using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class FileManagerStoragePolicyProvider :
    IFileManagerStoragePolicyProvider,
    ITransientDependency
{
    private readonly IFeatureChecker _featureChecker;
    private readonly IFileStorageQuota _fileStorageQuota;

    public FileManagerStoragePolicyProvider(
        IFeatureChecker featureChecker,
        IFileStorageQuota fileStorageQuota)
    {
        _featureChecker = featureChecker;
        _fileStorageQuota = fileStorageQuota;
    }

    public virtual async Task<FileManagerStoragePolicy> GetAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var providerValue = await _featureChecker.GetOrNullAsync(
            SufiFileManagerFeatures.Storage.Provider);
        if (!Enum.TryParse(
                providerValue,
                ignoreCase: true,
                out FileStructureStorageProvider provider))
        {
            provider = FileStructureStorageProvider.Database;
        }

        return new FileManagerStoragePolicy
        {
            Provider = provider,
            MaxStorageBytes = await _fileStorageQuota.GetLimitBytesAsync(cancellationToken)
        };
    }
}
