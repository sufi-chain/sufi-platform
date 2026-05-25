using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace SufiChain.SufiAbp.CLI.Templates;

/// <summary>
/// Manages template discovery and loading from ZIP files, embedded resources, and filesystem.
/// </summary>
public class TemplateManager
{
    private const string DefaultTemplateName = "blazor-webapp-layered-tiered";
    
    // CDN base URL for downloading templates (release mode)
    private const string CdnBaseUrl = "https://cdn.sabp.ir/sufi-abp";
    private const string CdnVersionManifestUrl = "https://cdn.sabp.ir/sufi-abp/latest.json";
    
    // Unified template name (single ZIP containing all variants)
    private const string UnifiedTemplateName = "app-blazor-webapp-unified";
    
    // Debug mode flag (checks if running from bin/Debug)
    private static readonly bool IsDebugMode = AppContext.BaseDirectory.Contains("Debug", StringComparison.OrdinalIgnoreCase);
    
    // Embedded template resource prefix
    private const string EmbeddedResourcePrefix = "SufiChain.SufiAbp.CLI.Templates.Embedded.";
    
    /// <summary>
    /// Gets all available templates.
    /// </summary>
    public List<TemplateInfo> GetAvailableTemplates()
    {
        var templates = new List<TemplateInfo>();
        
        // Add embedded templates
        templates.AddRange(GetEmbeddedTemplates());
        
        // Add filesystem templates
        templates.AddRange(GetFilesystemTemplates());
        
        return templates;
    }
    
    /// <summary>
    /// Gets templates from embedded resources.
    /// </summary>
    private List<TemplateInfo> GetEmbeddedTemplates()
    {
        var templates = new List<TemplateInfo>();
        var assembly = Assembly.GetExecutingAssembly();
        
        // Look for manifest files
        var manifestResources = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(EmbeddedResourcePrefix) && n.EndsWith(".manifest.json"))
            .ToList();

        foreach (var manifestResource in manifestResources)
        {
            using var stream = assembly.GetManifestResourceStream(manifestResource);
            if (stream == null) continue;
            
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var manifest = TemplateManifest.FromJson(json);
            
            if (manifest != null)
            {
                templates.Add(new TemplateInfo
                {
                    Name = manifest.Name,
                    Description = manifest.Description,
                    Source = "embedded",
                    SupportedDatabaseProviders = new List<string> { "EntityFrameworkCore", "MongoDB" },
                    SupportedArchitectures = new List<string> { "tiered", "single" }
                });
            }
        }
        
        // If no manifest found but we have embedded files, add a default template
        if (!templates.Any())
        {
            var hasEmbeddedFiles = assembly.GetManifestResourceNames()
                .Any(n => n.StartsWith(EmbeddedResourcePrefix) && !n.EndsWith(".manifest.json"));
                
            if (hasEmbeddedFiles)
            {
                templates.Add(new TemplateInfo
                {
                    Name = DefaultTemplateName,
                    Description = "Sufi Platform Blazor WebApp template",
                    Source = "embedded",
                    SupportedDatabaseProviders = new List<string> { "EntityFrameworkCore", "MongoDB" },
                    SupportedArchitectures = new List<string> { "tiered", "single" }
                });
            }
        }
        
        return templates;
    }
    
    /// <summary>
    /// Gets templates from filesystem (development mode).
    /// </summary>
    private List<TemplateInfo> GetFilesystemTemplates()
    {
        var templates = new List<TemplateInfo>();
        
        // Discover unified template from filesystem (Debug mode)
        var unifiedPath = GetUnifiedTemplatePath();
        if (!string.IsNullOrEmpty(unifiedPath) && Directory.Exists(unifiedPath) && Directory.GetFiles(unifiedPath, "*.sln").Length > 0)
        {
            // Add all architecture variants from unified template
            foreach (var arch in new[] { "single", "layered", "layered-tiered" })
            {
                templates.Add(new TemplateInfo
                {
                    Name = $"blazor-webapp-{arch}",
                    Description = $"Sufi Platform Blazor WebApp {arch} template (development)",
                    Source = "filesystem",
                    Path = unifiedPath,
                    SupportedDatabaseProviders = new List<string> { "EntityFrameworkCore", "MongoDB" },
                    SupportedArchitectures = new List<string> { arch }
                });
            }
        }
        
        // Try SOPHI_TEMPLATE_PATH environment variable
        var envPath = Environment.GetEnvironmentVariable("SOPHI_TEMPLATE_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            var name = Path.GetFileName(envPath);
            templates.Add(new TemplateInfo
            {
                Name = name,
                Description = $"Custom template from {envPath}",
                Source = "filesystem",
                Path = envPath,
                SupportedDatabaseProviders = new List<string> { "EntityFrameworkCore", "MongoDB" },
                SupportedArchitectures = new List<string> { "custom" }
            });
        }
        
        return templates;
    }
    
    /// <summary>
    /// Gets the unified template path (sufi-abp/templates/app/aspnet-core).
    /// This is the unified template structure containing all architecture variants.
    /// </summary>
    private string? GetUnifiedTemplatePath()
    {
        // Try from assembly location
        var assemblyLocation = typeof(TemplateManager).Assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var cliDir = Path.GetDirectoryName(assemblyLocation);
            if (cliDir != null)
            {
                // Core DLL: framework/SufiChain.SufiAbp.CLI.Core/bin/Debug/net10.0 -> repo root is 5 levels up
                var repoRoot = Path.GetFullPath(Path.Combine(cliDir, "..", "..", "..", "..", ".."));
                var unifiedPath = Path.Combine(repoRoot, "templates", "app", "aspnet-core");
                if (Directory.Exists(unifiedPath))
                {
                    return Path.GetFullPath(unifiedPath);
                }
            }
        }
        
        // Try from current directory
        var currentDir = Directory.GetCurrentDirectory();
        var fromCurrent = Path.GetFullPath(Path.Combine(currentDir, "templates", "app", "aspnet-core"));
        if (Directory.Exists(fromCurrent))
        {
            return fromCurrent;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the template path for the specified template name.
    /// Returns null if template not found.
    /// </summary>
    public string? GetTemplatePath(string templateName = DefaultTemplateName)
    {
        // 1. Try SOPHI_TEMPLATE_PATH environment variable first
        var envPath = Environment.GetEnvironmentVariable("SOPHI_TEMPLATE_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath) &&
            Directory.GetFiles(envPath, "*.sln").Length > 0)
        {
            return envPath;
        }
        
        // 2. In Debug mode, try unified template path first (new structure)
        if (IsDebugMode)
        {
            var unifiedPath = GetUnifiedTemplatePath();
            if (!string.IsNullOrEmpty(unifiedPath))
            {
                return unifiedPath;
            }
        }
        
        // 3. Return null to indicate download or embedded resources should be used
        return null;
    }
    
    /// <summary>
    /// Checks if embedded templates are available.
    /// </summary>
    public bool HasEmbeddedTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames()
            .Any(n => n.StartsWith(EmbeddedResourcePrefix));
    }
    
    /// <summary>
    /// Loads template files from embedded resources.
    /// </summary>
    public Dictionary<string, byte[]> LoadEmbeddedTemplate(string templateName = DefaultTemplateName)
    {
        var files = new Dictionary<string, byte[]>();
        var assembly = Assembly.GetExecutingAssembly();
        
        // Look for manifest
        var manifestResourceName = $"{EmbeddedResourcePrefix}{templateName.Replace("-", "_")}.manifest.json";
        using var manifestStream = assembly.GetManifestResourceStream(manifestResourceName);
        
        if (manifestStream != null)
        {
            // Load using manifest
            using var reader = new StreamReader(manifestStream);
            var manifest = TemplateManifest.FromJson(reader.ReadToEnd());
            
            if (manifest != null)
            {
                foreach (var entry in manifest.Files)
                {
                    using var stream = assembly.GetManifestResourceStream(entry.ResourceName);
                    if (stream == null) continue;
                    
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    files[entry.FilePath] = ms.ToArray();
                }
            }
        }
        else
        {
            // Fallback: Load all resources with template prefix and reconstruct paths
            var prefix = $"{EmbeddedResourcePrefix}{templateName.Replace("-", "_")}.";
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(prefix))
                .ToList();

            foreach (var resourceName in resourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                
                // Reconstruct file path from resource name
                var relativePath = ReconstructFilePath(resourceName, prefix);
                files[relativePath] = ms.ToArray();
            }
        }
        
        return files;
    }
    
    
    /// <summary>
    /// Reconstructs the original file path from an embedded resource name.
    /// </summary>
    private string ReconstructFilePath(string resourceName, string prefix)
    {
        // Remove prefix
        var path = resourceName.Substring(prefix.Length);
        
        // Common file extensions to handle
        var extensions = new Dictionary<string, string>
        {
            { ".cs", ".cs" },
            { ".razor", ".razor" },
            { ".csproj", ".csproj" },
            { ".json", ".json" },
            { ".props", ".props" },
            { ".sln", ".sln" },
            { ".md", ".md" },
            { ".css", ".css" },
            { ".js", ".js" },
            { ".ts", ".ts" },
            { ".html", ".html" },
            { ".xml", ".xml" },
            { ".targets", ".targets" },
            { ".yml", ".yml" },
            { ".yaml", ".yaml" },
            { ".txt", ".txt" },
            { ".gitignore", ".gitignore" },
            { ".editorconfig", ".editorconfig" }
        };
        
        // Find the extension at the end of the path
        foreach (var ext in extensions)
        {
            var extSuffix = ext.Key.Replace(".", "_");
            if (path.EndsWith(extSuffix))
            {
                // Replace extension suffix
                path = path.Substring(0, path.Length - extSuffix.Length) + ext.Value;
                break;
            }
        }
        
        // Replace underscores with path separators, but be careful with:
        // - Double underscores (_) which might be intentional
        // - Underscores that are part of file/folder names
        
        // First, handle special cases like _Imports_razor -> _Imports.razor
        path = path.Replace("__", "\x00"); // Placeholder for double underscore
        
        // Convert dots to path separators (embedded resources use dots)
        path = path.Replace(".", "\\");
        
        // Restore double underscores
        path = path.Replace("\x00", "_");
        
        // Handle hidden files (they start with .)
        if (path.Contains("\\_"))
        {
            path = path.Replace("\\_", "\\.");
        }
        
        return path;
    }
    
    #region ZIP Template Support
    
    /// <summary>
    /// Gets the path to a local template ZIP file.
    /// Returns null if no local ZIP is available.
    /// </summary>
    public string? GetLocalZipPath(string templateName = DefaultTemplateName)
    {
        // 1. Check SOPHI_TEMPLATE_ZIP environment variable
        var envZipPath = Environment.GetEnvironmentVariable("SOPHI_TEMPLATE_ZIP");
        if (!string.IsNullOrEmpty(envZipPath) && File.Exists(envZipPath))
        {
            return envZipPath;
        }
        
        // 2. Check templates directory relative to CLI assembly
        var assemblyLocation = typeof(TemplateManager).Assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var cliDir = Path.GetDirectoryName(assemblyLocation);
            if (cliDir != null)
            {
                var repoRoot = Path.GetFullPath(Path.Combine(cliDir, "..", "..", "..", "..", "..", ".."));
                // Check templates/ directory
                var srcTemplatesPath = Path.Combine(repoRoot, "templates");
                var zipPath = Path.Combine(srcTemplatesPath, $"{templateName}.zip");
                if (File.Exists(zipPath))
                {
                    return zipPath;
                }
            }
        }
        
        // 3. Walk up from current directory looking for templates
        var currentDir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 10; i++)
        {
            var templatesDir = Path.Combine(currentDir, "templates");
            var zipPath = Path.Combine(templatesDir, $"{templateName}.zip");
            if (File.Exists(zipPath))
            {
                return zipPath;
            }
            
            var parent = Directory.GetParent(currentDir);
            if (parent == null) break;
            currentDir = parent.FullName;
        }
        
        return null;
    }
    
    /// <summary>
    /// Downloads a template ZIP from GitHub releases.
    /// Returns the path to the downloaded ZIP file.
    /// </summary>
    public async Task<string> DownloadTemplateAsync(
        string templateName = DefaultTemplateName, 
        string? version = null,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // First check for local ZIP (dev mode)
        var localZip = GetLocalZipPath(templateName);
        if (localZip != null)
        {
            return localZip;
        }
        
        // Build download URL
        var manifest = await FetchLatestVersionAsync(cancellationToken);
        var releaseVersion = version ?? manifest?.Version ?? "1.0.0-alpha.1.0";
        var url = $"{CdnBaseUrl}/{releaseVersion}/templates/{templateName}.zip";
        
        // Download to temp location
        var tempPath = Path.Combine(Path.GetTempPath(), "sufi-templates");
        Directory.CreateDirectory(tempPath);
        var destPath = Path.Combine(tempPath, $"{templateName}-{releaseVersion}.zip");
        
        // Check if already downloaded
        if (File.Exists(destPath))
        {
            return destPath;
        }
        
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        try
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0 && progress != null)
                {
                    var percentage = (int)((downloadedBytes * 100) / totalBytes);
                    progress.Report(percentage);
                }
            }
            
            return destPath;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to download template from {url}. " +
                $"Make sure the release exists and you have internet connectivity. " +
                $"Error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Extracts a template ZIP file to a dictionary of file paths and contents.
    /// </summary>
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
            // Skip directories (they have empty names ending with /)
            if (string.IsNullOrEmpty(entry.Name) || entry.FullName.EndsWith("/"))
            {
                continue;
            }
            
            // Read file content
            using var entryStream = entry.Open();
            using var memoryStream = new MemoryStream();
            entryStream.CopyTo(memoryStream);
            
            // Normalize path separators to use backslash (Windows-style for consistency)
            var normalizedPath = entry.FullName.Replace('/', '\\');
            
            files[normalizedPath] = memoryStream.ToArray();
        }
        
        return files;
    }
    
    /// <summary>
    /// Gets the best available template source and loads it.
    /// Priority: Environment variable > Debug: Unified template > Local ZIP > Release: CDN download > Embedded
    /// </summary>
    public async Task<Dictionary<string, byte[]>> LoadTemplateAsync(
        string templateName = DefaultTemplateName,
        IProgress<int>? downloadProgress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Try environment variable (filesystem path or ZIP)
        var envPath = Environment.GetEnvironmentVariable("SOPHI_TEMPLATE_PATH");
        if (!string.IsNullOrEmpty(envPath))
        {
            if (envPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && File.Exists(envPath))
            {
                return ExtractTemplateZip(envPath);
            }
            if (Directory.Exists(envPath))
            {
                return LoadFromFilesystem(envPath);
            }
        }
        
        // 2. Try local ZIP files in templates/ (Debug mode)
        if (IsDebugMode)
        {
            var localZip = GetLocalZipPath(templateName);
            if (localZip != null)
            {
                return ExtractTemplateZip(localZip);
            }
        }
        
        // 3. Fallback: Try unified template path (filesystem - for development without ZIP)
        var unifiedPath = GetUnifiedTemplatePath();
        if (!string.IsNullOrEmpty(unifiedPath) && Directory.Exists(unifiedPath))
        {
            return LoadFromFilesystem(unifiedPath);
        }
        
        // 4. In Release mode, try downloading from CDN
        if (!IsDebugMode)
        {
        try
        {
                var downloadedZip = await DownloadFromCdnAsync(templateName, downloadProgress, cancellationToken);
            return ExtractTemplateZip(downloadedZip);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
                // Download failed, continue to embedded
            }
        }
        
        // 5. Try embedded as last resort
            if (HasEmbeddedTemplates())
            {
                return LoadEmbeddedTemplate(templateName);
            }
            
        // No template found
            throw new InvalidOperationException(
                $"Failed to load template '{templateName}'. " +
                $"No template found in unified path, embedded resources, or CDN. " +
                $"In Debug mode, ensure sufi-abp/templates/app/aspnet-core/ exists. " +
                $"In Release mode, ensure internet connectivity for CDN download from https://cdn.sabp.ir/sufi-abp/latest.json");
    }
    
    /// <summary>
    /// Downloads template from CDN with dynamic version discovery.
    /// URL format: https://cdn.sabp.ir/sufi-abp/{version}/templates/app-blazor-webapp-unified.zip
    /// </summary>
    /// <summary>
    /// Fetches the latest version manifest from CDN.
    /// </summary>
    private static async Task<CdnVersionManifest?> FetchLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var json = await client.GetStringAsync(CdnVersionManifestUrl, cancellationToken);
            return JsonSerializer.Deserialize<CdnVersionManifest>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Downloads template from CDN with dynamic version discovery.
    /// </summary>
    private async Task<string> DownloadFromCdnAsync(
        string templateName,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Fetch latest version from CDN
        var manifest = await FetchLatestVersionAsync(cancellationToken);
        if (manifest == null || string.IsNullOrEmpty(manifest.Version))
        {
            throw new InvalidOperationException("Failed to fetch template version from CDN. Please check your internet connection.");
        }
        
        // Get unified template URL from manifest
        if (!manifest.Templates.TryGetValue(UnifiedTemplateName, out var templateInfo))
        {
            throw new InvalidOperationException($"Template '{UnifiedTemplateName}' not found in CDN manifest.");
        }
        
        var url = templateInfo.Url;
        var tempPath = Path.Combine(Path.GetTempPath(), $"sufiabp-template-{manifest.Version}-{Guid.NewGuid()}.zip");
        
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        try
        {
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0 && progress != null)
                {
                    var percentage = (int)((downloadedBytes * 100) / totalBytes);
                    progress.Report(percentage);
                }
            }
            
            return tempPath;
        }
        catch (Exception ex)
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
            throw new InvalidOperationException($"Failed to download template from CDN: {url}. Error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Loads template files from a filesystem directory.
    /// </summary>
    public Dictionary<string, byte[]> LoadFromFilesystem(string directoryPath)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Template directory not found: {directoryPath}");
        }
        
        var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
        
        foreach (var filePath in allFiles)
        {
            // Skip build artifacts and hidden folders
            var relativePath = Path.GetRelativePath(directoryPath, filePath);
            if (ShouldSkipFile(relativePath))
            {
                continue;
            }
            
            files[relativePath] = File.ReadAllBytes(filePath);
        }
        
        return files;
    }
    
    /// <summary>
    /// Determines if a file should be skipped during template loading.
    /// </summary>
    private static bool ShouldSkipFile(string relativePath)
    {
        var pathLower = relativePath.ToLowerInvariant();
        
        // Skip build artifacts
        if (pathLower.Contains("\\bin\\") || pathLower.Contains("\\obj\\"))
            return true;
        
        // Skip IDE folders
        if (pathLower.Contains("\\.vs\\") || pathLower.Contains("\\.idea\\"))
            return true;
        
        // Skip node_modules
        if (pathLower.Contains("\\node_modules\\"))
            return true;
        
        // Skip user-specific files
        if (pathLower.EndsWith(".user") || pathLower.EndsWith(".suo"))
            return true;
        
        return false;
    }
    
    #endregion
}
