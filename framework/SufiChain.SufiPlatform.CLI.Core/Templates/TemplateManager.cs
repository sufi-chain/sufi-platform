using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace SufiChain.SufiPlatform.CLI.Templates;

/// <summary>
/// Loads versioned Sufi Platform templates using the same source model as ABP CLI:
/// local template source for development, CDN zip source for releases, and cache for downloaded zips.
/// </summary>
public class TemplateManager
{
    public const string DefaultTemplateName = "app-blazor-webapp-unified";

    private const string CdnBaseUrl = "https://cdn.sufiplatform.com/templates";
    private const string LatestVersionFileName = "latest.json";
    private const string LocalTemplateEnvironmentVariable = "SOPHI_TEMPLATE_PATH";
    private const string LocalTemplateZipEnvironmentVariable = "SOPHI_TEMPLATE_ZIP";

    private static readonly bool IsDebugMode =
#if DEBUG
        true;
#else
        AppContext.BaseDirectory.Contains("Debug", StringComparison.OrdinalIgnoreCase);
#endif

    /// <summary>
    /// Gets all known template entries.
    /// </summary>
    public List<TemplateInfo> GetAvailableTemplates()
    {
        return new List<TemplateInfo>
        {
            new()
            {
                Name = DefaultTemplateName,
                Description = "Sufi Platform Blazor WebApp unified template",
                Source = IsDebugMode ? "filesystem" : "cdn",
                Path = IsDebugMode ? GetDebugTemplateDirectory() ?? string.Empty : $"{CdnBaseUrl}/",
                SupportedDatabaseProviders = new List<string> { "EntityFrameworkCore", "MongoDB" },
                SupportedArchitectures = new List<string> { "webapp", "layered", "layered-tiered" }
            }
        };
    }

    /// <summary>
    /// Loads template files from the configured source.
    /// Debug uses sufi-platform/templates by default. Release uses cdn.sufiplatform.com/templates zips.
    /// </summary>
    public async Task<Dictionary<string, byte[]>> LoadTemplateAsync(
        string templateName,
        IProgress<int>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        templateName = NormalizeTemplateName(templateName);

        var explicitZip = Environment.GetEnvironmentVariable(LocalTemplateZipEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(explicitZip))
        {
            return ExtractTemplateZip(explicitZip);
        }

        var explicitTemplatePath = Environment.GetEnvironmentVariable(LocalTemplateEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(explicitTemplatePath))
        {
            return explicitTemplatePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? ExtractTemplateZip(explicitTemplatePath)
                : LoadFromFilesystem(ResolveTemplateDirectory(explicitTemplatePath));
        }

        if (IsDebugMode)
        {
            var debugTemplateDirectory = GetDebugTemplateDirectory();
            if (!string.IsNullOrWhiteSpace(debugTemplateDirectory))
            {
                return LoadFromFilesystem(debugTemplateDirectory);
            }
        }

        var templateZip = await GetTemplateZipFromCdnAsync(
            templateName,
            downloadProgress,
            cancellationToken);

        return ExtractTemplateZip(templateZip);
    }

    /// <summary>
    /// Kept for compatibility with older pipeline construction. The new loader chooses the source itself.
    /// </summary>
    public string? GetTemplatePath(string templateName)
    {
        var explicitTemplatePath = Environment.GetEnvironmentVariable(LocalTemplateEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(explicitTemplatePath) && Directory.Exists(explicitTemplatePath))
        {
            return ResolveTemplateDirectory(explicitTemplatePath);
        }

        return IsDebugMode ? GetDebugTemplateDirectory() : null;
    }

    public Dictionary<string, byte[]> ExtractTemplateZip(string zipPath)
    {
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException($"Template ZIP not found: {zipPath}");
        }

        var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name) || entry.FullName.EndsWith("/", StringComparison.Ordinal))
            {
                continue;
            }

            using var entryStream = entry.Open();
            using var memoryStream = new MemoryStream();
            entryStream.CopyTo(memoryStream);

            files[NormalizeEntryPath(entry.FullName)] = memoryStream.ToArray();
        }

        return files;
    }

    public Dictionary<string, byte[]> LoadFromFilesystem(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Template directory not found: {directoryPath}");
        }

        var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(directoryPath, filePath);
            if (ShouldSkipFile(relativePath))
            {
                continue;
            }

            files[NormalizeEntryPath(relativePath)] = File.ReadAllBytes(filePath);
        }

        return files;
    }

    private async Task<string> GetTemplateZipFromCdnAsync(
        string templateName,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        var manifest = await FetchLatestVersionAsync(cancellationToken);
        var version = manifest?.Version;

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException(
                $"Could not resolve the latest Sufi template version from {GetLatestManifestUrl()}.");
        }

        var cdnTemplate = ResolveCdnTemplate(manifest!, templateName, version);
        var cacheFile = GetCacheFilePath(templateName, version);

        if (File.Exists(cacheFile) && IsCachedFileValid(cacheFile, cdnTemplate))
        {
            return cacheFile;
        }

        var downloadUrl = string.IsNullOrWhiteSpace(cdnTemplate.Url)
            ? GetDefaultTemplateUrl(templateName, version)
            : cdnTemplate.Url;

        Directory.CreateDirectory(Path.GetDirectoryName(cacheFile)!);
        var tempFile = $"{cacheFile}.{Guid.NewGuid():N}.tmp";

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? cdnTemplate.Size;
            var downloadedBytes = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    progress?.Report((int)Math.Min(100, downloadedBytes * 100 / totalBytes));
                }
            }

            ValidateDownloadedFile(tempFile, cdnTemplate);

            if (File.Exists(cacheFile))
            {
                File.Delete(cacheFile);
            }

            File.Move(tempFile, cacheFile);
            return cacheFile;
        }
        catch
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            throw;
        }
    }

    private static async Task<CdnVersionManifest?> FetchLatestVersionAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var json = await httpClient.GetStringAsync(GetLatestManifestUrl(), cancellationToken);

        return JsonSerializer.Deserialize<CdnVersionManifest>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    private static CdnTemplateInfo ResolveCdnTemplate(CdnVersionManifest manifest, string templateName, string version)
    {
        if (manifest.Templates.TryGetValue(templateName, out var templateInfo))
        {
            return templateInfo;
        }

        if (manifest.Templates.TryGetValue(DefaultTemplateName, out var defaultTemplateInfo))
        {
            return defaultTemplateInfo;
        }

        return new CdnTemplateInfo
        {
            Url = GetDefaultTemplateUrl(templateName, version)
        };
    }

    private static bool IsCachedFileValid(string cacheFile, CdnTemplateInfo templateInfo)
    {
        if (templateInfo.Size > 0 && new FileInfo(cacheFile).Length != templateInfo.Size)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(templateInfo.Sha256))
        {
            return string.Equals(
                ComputeSha256(cacheFile),
                templateInfo.Sha256,
                StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private static void ValidateDownloadedFile(string filePath, CdnTemplateInfo templateInfo)
    {
        if (templateInfo.Size > 0 && new FileInfo(filePath).Length != templateInfo.Size)
        {
            throw new InvalidOperationException("Downloaded template size does not match CDN manifest.");
        }

        if (!string.IsNullOrWhiteSpace(templateInfo.Sha256) &&
            !string.Equals(ComputeSha256(filePath), templateInfo.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Downloaded template SHA256 does not match CDN manifest.");
        }
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetCacheFilePath(string templateName, string version)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".sufi",
            "templates",
            $"{templateName}-{version}.zip");
    }

    private static string? GetDebugTemplateDirectory()
    {
        var candidates = GetRepositoryRootCandidates()
            .Select(root => Path.Combine(root, "templates"))
            .Select(ResolveTemplateDirectory)
            .Where(Directory.Exists)
            .ToList();

        return candidates.FirstOrDefault();
    }

    private static IEnumerable<string> GetRepositoryRootCandidates()
    {
        var assemblyLocation = typeof(TemplateManager).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(assemblyLocation))
        {
            var current = Directory.GetParent(Path.GetDirectoryName(assemblyLocation)!);
            for (var i = 0; current != null && i < 10; i++, current = current.Parent)
            {
                yield return current.FullName;
            }
        }

        var workingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; workingDirectory != null && i < 10; i++, workingDirectory = workingDirectory.Parent)
        {
            yield return workingDirectory.FullName;
        }
    }

    private static string ResolveTemplateDirectory(string directoryPath)
    {
        var fullPath = Path.GetFullPath(directoryPath);
        var appAspNetCorePath = Path.Combine(fullPath, "app", "aspnet-core");

        return Directory.Exists(appAspNetCorePath)
            ? appAspNetCorePath
            : fullPath;
    }

    private static string NormalizeTemplateName(string templateName)
    {
        return string.IsNullOrWhiteSpace(templateName) ||
               templateName.Equals("blazor-webapp", StringComparison.OrdinalIgnoreCase) ||
               templateName.StartsWith("blazor-webapp-", StringComparison.OrdinalIgnoreCase)
            ? DefaultTemplateName
            : templateName;
    }

    private static string GetLatestManifestUrl()
    {
        return $"{CdnBaseUrl}/{LatestVersionFileName}";
    }

    private static string GetDefaultTemplateUrl(string templateName, string version)
    {
        return $"{CdnBaseUrl}/{version}/{templateName}.zip";
    }

    private static string NormalizeEntryPath(string path)
    {
        return path.Replace('/', '\\');
    }

    private static bool ShouldSkipFile(string relativePath)
    {
        var path = NormalizeEntryPath(relativePath).ToLowerInvariant();

        return path.Contains("\\bin\\", StringComparison.Ordinal) ||
               path.Contains("\\obj\\", StringComparison.Ordinal) ||
               path.Contains("\\.git\\", StringComparison.Ordinal) ||
               path.Contains("\\.vs\\", StringComparison.Ordinal) ||
               path.Contains("\\.idea\\", StringComparison.Ordinal) ||
               path.Contains("\\node_modules\\", StringComparison.Ordinal) ||
               path.Contains("\\logs\\", StringComparison.Ordinal) ||
               path.EndsWith(".user", StringComparison.Ordinal) ||
               path.EndsWith(".suo", StringComparison.Ordinal) ||
               path.EndsWith(".log", StringComparison.Ordinal) ||
               path.EndsWith("logs.txt", StringComparison.Ordinal);
    }
}
