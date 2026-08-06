using System;
using System.Globalization;
using System.IO;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Calculates blob names for file-manager storage.
/// Standard path: {year}/{month}/{fileId}.ext — combined with BasePath (assets/{structure}|{custom})
/// and ABP's host/tenant segment gives: assets/{structure}/{host|tenant}/{year}/{month}/file.
/// Uses InvariantCulture for date formatting so blob paths are culture-independent (avoids Persian/Jalali vs Gregorian mismatch).
/// </summary>
public class FileBlobNameCalculator : IFileBlobNameCalculator, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;

    public FileBlobNameCalculator(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }

    public string Calculate(
        Guid fileId,
        string fileName,
        bool isTemp,
        string? structureKey = null)
    {
        var extension = Path.GetExtension(fileName);
        var tenantPrefix = _currentTenant.Id.HasValue ? _currentTenant.Id.Value.ToString("D", CultureInfo.InvariantCulture) : "host";

        if (isTemp)
        {
            return $"{tenantPrefix}/temp/{fileId}{extension}";
        }

        var now = DateTime.UtcNow;
        // Blob name: year/month/fileId.ext — use InvariantCulture so path is always Gregorian (2025/02), not Persian (1404/12)
        return now.ToString("yyyy/MM", CultureInfo.InvariantCulture) + $"/{fileId}{extension}";
    }
}
