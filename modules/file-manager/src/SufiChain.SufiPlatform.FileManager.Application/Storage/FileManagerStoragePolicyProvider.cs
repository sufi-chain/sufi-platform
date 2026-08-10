using System;
using System.Globalization;
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

    public FileManagerStoragePolicyProvider(IFeatureChecker featureChecker)
    {
        _featureChecker = featureChecker;
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

        var maximumBytesValue = await _featureChecker.GetOrNullAsync(
            SufiFileManagerFeatures.Storage.MaxBytes);
        if (!long.TryParse(
                maximumBytesValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var maximumBytes) ||
            maximumBytes < 0)
        {
            maximumBytes = long.Parse(
                SufiFileManagerFeatures.Storage.DefaultMaxBytes,
                CultureInfo.InvariantCulture);
        }

        return new FileManagerStoragePolicy
        {
            Provider = provider,
            MaxStorageBytes = maximumBytes
        };
    }
}
