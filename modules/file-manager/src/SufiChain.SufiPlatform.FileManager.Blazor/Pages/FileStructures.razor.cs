using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Components.Overlays;
using SufiChain.SufiPlatform.FileManager.FileMigration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Pages;

public partial class FileStructures : FileManagerComponentBase, IDisposable
{

    [Inject] private IFileStructureAppService FileStructureAppService { get; set; } = default!;
    [Inject] private IFileMigrationAppService FileMigrationAppService { get; set; } = default!;
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private bool _loading = true;
    private string _searchTerm = "";
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed = false;
    
    private List<FileStructureDto> _structures = new();
    private bool _viewModalOpen;
    private SbConfirmDialog? _deleteConfirm;
    private SbConfirmDialog? _resetConfirm;
    private FileStructureDto? _structureToDelete;
    private FileStructureDto? _structureToReset;
    
    private FileStructureDto? _viewStructure;
    private FileStructureDefaultDto? _defaultConfig;


    private IEnumerable<FileStructureDto> FilteredStructures =>
        string.IsNullOrWhiteSpace(_searchTerm)
            ? _structures
            : _structures.Where(s =>
                s.Key.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                s.DisplayName.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (s.Description?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadStructures();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["FileStructures"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private async Task LoadStructures()
    {
        _loading = true;
        StateHasChanged(); // Show loading state
        try
        {
            var result = await FileStructureAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = "Key"
            });
            _structures = result.Items.ToList();
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "FailedToLoadStructures");
        }
        finally
        {
            _loading = false;
            StateHasChanged(); // Update UI with results
        }
    }

    private void OnSearchChanged(ChangeEventArgs e)
    {
        _searchTerm = e.Value?.ToString() ?? "";
    }

    private void OpenCreatePage()
    {
        NavigationManager.NavigateTo(FileStructureWizard.ListRoute + "/new");
    }

    private void OpenEditPage(FileStructureDto structure)
    {
        NavigationManager.NavigateTo($"{FileStructureWizard.ListRoute}/{structure.Id}/edit");
    }


    private async Task OpenViewModal(FileStructureDto structure)
    {
        _viewStructure = structure;
        
        if (structure.HasDefaultConfig)
        {
            _defaultConfig = await FileStructureAppService.GetDefaultConfigAsync(structure.Key);
        }
        else
        {
            _defaultConfig = null;
        }
        
        _viewModalOpen = true;
    }

    private Task CloseViewModal()
    {
        _viewModalOpen = false;
        return Task.CompletedTask;
    }

    private async Task MigrateStructureFilesAsync(FileStructureDto structure)
    {
        try
        {
            var result = await FileMigrationAppService.MigrateStructureAsync(new StartStructureMigrationInput
            {
                StructureKey = structure.Key,
                DeleteSourceAfterVerify = true
            });
            await Notify.SuccessAsync(L["MigrateStructureFilesResult", result.Migrated, result.Skipped, result.Failed]);
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "FailedToMigrateStructureFiles");
        }
    }

    private void ResetToDefault(FileStructureDto structure)
    {
        _structureToReset = structure;
        _resetConfirm?.Show();
    }

    private void CancelReset()
    {
        _structureToReset = null;
    }

    private async Task ConfirmReset()
    {
        if (_structureToReset == null) return;

        try
        {
            await FileStructureAppService.ResetToDefaultAsync(_structureToReset.Id);
            await Notify.SuccessAsync(L["StructureResetSuccessfully"]);
            await CloseViewModal();
            await LoadStructures();
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "FailedToResetStructure");
        }
        finally
        {
            _structureToReset = null;
        }
    }

    private void DeleteStructure(FileStructureDto structure)
    {
        _structureToDelete = structure;
        _deleteConfirm?.Show();
    }

    private void CancelDeleteStructure()
    {
        _structureToDelete = null;
    }

    private async Task ConfirmDeleteStructure()
    {
        if (_structureToDelete == null) return;

        try
        {
            await FileStructureAppService.DeleteAsync(_structureToDelete.Id);
            await Notify.SuccessAsync(L["StructureDeletedSuccessfully"]);
            await LoadStructures();
        }
        catch (Exception ex)
        {
            await NotifyOperationFailedAsync(ex, "FailedToDeleteStructure");
        }
        finally
        {
            _structureToDelete = null;
        }
    }


    private static List<string> ParseCsvToList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
    }


    private List<string> GetFileTypes(FileType types)
    {
        var result = new List<string>();
        if (types.HasFlag(FileType.Image)) result.Add("FileTypeImage");
        if (types.HasFlag(FileType.Video)) result.Add("FileTypeVideo");
        if (types.HasFlag(FileType.Document)) result.Add("FileTypeDocument");
        if (types.HasFlag(FileType.Audio)) result.Add("FileTypeAudio");
        return result;
    }

    private SbColor GetFileTypeColor(string typeKey) => typeKey switch
    {
        "FileTypeImage" => SbColor.Success,
        "FileTypeVideo" => SbColor.Info,
        "FileTypeDocument" => SbColor.Warning,
        "FileTypeAudio" => SbColor.Primary,
        _ => SbColor.Default
    };

    private string FormatSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private string TruncateText(string text, int maxLength) =>
        text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";


    private string GetStorageProviderLabel(FileStructureStorageProvider provider) =>
        provider switch
        {
            FileStructureStorageProvider.Database => L["StorageProviderDatabase"],
            FileStructureStorageProvider.FileSystem => L["StorageProviderFileSystem"],
            FileStructureStorageProvider.MinIO => L["StorageProviderMinIO"],
            FileStructureStorageProvider.S3Provider => L["StorageProviderS3"],
            _ => provider.ToString()
        };

    private string GetStorageProviderDisplay(FileStructureStorageProvider? provider, string? providerStr)
    {
        if (provider.HasValue) return GetStorageProviderLabel(provider.Value);
        if (!string.IsNullOrEmpty(providerStr) && Enum.TryParse<FileStructureStorageProvider>(providerStr, out var p))
            return GetStorageProviderLabel(p);
        return providerStr ?? "-";
    }

    private string FormatImageDimensions(int? minW, int? minH, int? maxW, int? maxH)
    {
        var hasMin = (minW ?? 0) > 0 || (minH ?? 0) > 0;
        var hasMax = (maxW ?? 0) > 0 || (maxH ?? 0) > 0;
        if (!hasMin && !hasMax) return "-";
        var parts = new List<string>();
        if (hasMin) parts.Add($"Min: {minW ?? 0}×{minH ?? 0}");
        if (hasMax) parts.Add($"Max: {maxW ?? 0}×{maxH ?? 0}");
        return string.Join(", ", parts) + " px";
    }

    private string FormatStorageConfigDetails(FileStructureStorageConfigDto? config, string? structureKey = null)
    {
        if (config == null) return "-";
        return config.StorageProvider switch
        {
            FileStructureStorageProvider.Database => config.HasDatabaseConnectionString ? L["SensitiveValueConfigured"] : "-",
            FileStructureStorageProvider.FileSystem => FormatFileSystemPath(config.FileSystemBasePath, structureKey),
            FileStructureStorageProvider.MinIO => string.IsNullOrEmpty(config.MinioEndPoint) ? "-" : $"{config.MinioEndPoint} / {config.MinioBucketName ?? "?"}",
            FileStructureStorageProvider.S3Provider => string.IsNullOrEmpty(config.S3EndPoint)
                ? (string.IsNullOrEmpty(config.S3ContainerName) ? "-" : $"AWS S3 / {config.S3ContainerName}")
                : $"{config.S3EndPoint} / {config.S3ContainerName ?? "?"}",
            _ => "-"
        };
    }

    private string FormatFileSystemPath(string? customPath, string? structureKey)
    {
        //Todo this must be configured by ops from ui
        const string prefix = "assets";
        if (string.IsNullOrWhiteSpace(customPath))
        {
            var structure = string.IsNullOrEmpty(structureKey) ? "..." : structureKey.Replace(".", "-", StringComparison.Ordinal).ToLowerInvariant();
            return $"{prefix}/{structure}/{{host|tenant}}/{{year}}/{{month}}";
        }
        var path = customPath.Trim().Replace('\\', '/').Trim('/');
        return string.IsNullOrEmpty(path) ? $"{prefix}/{{host|tenant}}/{{year}}/{{month}}" : $"{prefix}/{path}/{{host|tenant}}/{{year}}/{{month}}";
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cts.Cancel();
        _cts.Dispose();
    }
}
