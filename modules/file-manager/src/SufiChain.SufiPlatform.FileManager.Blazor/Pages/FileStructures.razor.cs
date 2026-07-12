using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SufiChain.SufiBlazor.Components;
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
    [Inject] private IFileManagerStorageSettingsAppService StorageSettingsAppService { get; set; } = default!;
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private bool _loading = true;
    private bool _saving = false;
    private bool _testingConnection = false;
    private bool _isEditing = false;
    private string _searchTerm = "";
    private string? _selectedTab = "basic";
    private bool _showExtensionDetails = false;
    private List<string> _extensionTags = new();
    private List<string> _mimeTypeTags = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed = false;
    
    private List<FileStructureDto> _structures = new();
    private bool _editModalOpen;
    private bool _createWizardOpen;
    private int _wizardStep;
    private EditContext? _createEditContext;
    private bool _viewModalOpen;
    private bool _deleteConfirmOpen;
    private bool _resetConfirmOpen;
    private FileStructureDto? _structureToDelete;
    private FileStructureDto? _structureToReset;
    
    private CreateUpdateFileStructureDto _editModel = new();
    private Guid? _editingId;

    private const int WizardStepCount = 7;

    private static readonly FileStructureStorageProvider[] _storageProviderOptions =
    {
        FileStructureStorageProvider.Database,
        FileStructureStorageProvider.FileSystem,
        FileStructureStorageProvider.MinIO,
        FileStructureStorageProvider.S3Provider
    };
    private bool _showDbConnectionString;
    private bool _showMinioAccessKey;
    private bool _showMinioSecretKey;
    private bool _showS3AccessKey;
    private bool _showS3SecretKey;

    private FileStructureDto? _viewStructure;
    private FileStructureDefaultDto? _defaultConfig;

    private FileStructureStorageConfigDto? _defaultStorageConfig;
    private bool _hasDefaultStorageConfigured;

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
            await Notify.ErrorAsync(L["FailedToLoadStructures", ex.Message]);
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

    private Task OpenCreateModal()
    {
        _isEditing = false;
        _editingId = null;
        _wizardStep = 0;
        _selectedTab = "basic";
        _showExtensionDetails = false;
        _editModel = new CreateUpdateFileStructureDto
        {
            AllowedFileTypes = FileType.Image,
            AllowedExtensions = "jpg,jpeg,png,gif,webp",
            AllowedMimeTypes = "image/jpeg,image/png,image/gif,image/webp",
            MaxFileSize = 10 * 1024 * 1024,
            GenerateThumbnail = true,
            ThumbnailWidth = 200,
            ThumbnailHeight = 200,
            WebPQuality = 80,
            StorageConfig = new FileStructureStorageConfigDto { StorageProvider = FileStructureStorageProvider.Database }
        };
        _createEditContext = new EditContext(_editModel);
        _showDbConnectionString = false;
        _showMinioAccessKey = false;
        _showMinioSecretKey = false;
        _extensionTags = ParseCsvToList(_editModel.AllowedExtensions);
        _mimeTypeTags = ParseCsvToList(_editModel.AllowedMimeTypes);
        _defaultStorageConfig = null;
        _hasDefaultStorageConfigured = false;
        _createWizardOpen = true;
        return Task.CompletedTask;
    }

    private void CloseCreateWizard()
    {
        _createWizardOpen = false;
    }

    private async Task WizardNext()
    {
        if (!await ValidateCurrentStepAsync())
            return;
        _wizardStep = Math.Min(_wizardStep + 1, WizardStepCount - 1);
        if (_wizardStep == 5)
            await LoadDefaultStorageConfigAsync();
    }

    private void WizardBack()
    {
        _wizardStep = Math.Max(0, _wizardStep - 1);
    }

    private async Task WizardFinish()
    {
        if (_createEditContext == null || !_createEditContext.Validate())
            return;
        await SaveStructureFromWizard();
    }

    private async Task LoadDefaultStorageConfigAsync()
    {
        try
        {
            _defaultStorageConfig = await StorageSettingsAppService.GetDefaultConfigAsync();
            _hasDefaultStorageConfigured = ComputeHasDefaultStorageConfigured(_defaultStorageConfig);
            if (_editModel.StorageConfig != null && _defaultStorageConfig != null)
            {
                _editModel.StorageConfig.StorageProvider = _defaultStorageConfig.StorageProvider;
            }
        }
        catch
        {
            _defaultStorageConfig = null;
            _hasDefaultStorageConfigured = false;
        }
        StateHasChanged();
    }

    private static bool ComputeHasDefaultStorageConfigured(FileStructureStorageConfigDto? config)
    {
        if (config == null) return false;
        return config.StorageProvider switch
        {
            FileStructureStorageProvider.Database => true,
            FileStructureStorageProvider.FileSystem => true,
            FileStructureStorageProvider.MinIO => !string.IsNullOrWhiteSpace(config.MinioEndPoint) && !string.IsNullOrWhiteSpace(config.MinioBucketName),
            FileStructureStorageProvider.S3Provider => !string.IsNullOrWhiteSpace(config.S3Region) && !string.IsNullOrWhiteSpace(config.S3ContainerName),
            _ => false
        };
    }

    private async Task<bool> ValidateCurrentStepAsync()
    {
        if (_createEditContext == null) return true;

        if (_wizardStep == 1) // Step 2 Identity
        {
            _createEditContext.Validate();
            if (_createEditContext.GetValidationMessages().Any())
                return false;
            return true;
        }

        if (_wizardStep == 2) // Step 3 File Types
        {
            if (_editModel.AllowedFileTypes == 0)
            {
                await Notify.WarnAsync(L["CreateWizard:ValidationFileTypeRequired"].Value ?? "Select at least one file type.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_editModel.AllowedExtensions) || string.IsNullOrWhiteSpace(_editModel.AllowedMimeTypes))
            {
                await Notify.WarnAsync(L["CreateWizard:ValidationFileTypeRequired"].Value ?? "Select at least one file type.");
                return false;
            }
            return true;
        }

        if (_wizardStep == 3) // Step 4 File Settings
        {
            const long oneMB = 1024 * 1024;
            if (_editModel.MaxFileSize < oneMB)
            {
                await Notify.WarnAsync(L["CreateWizard:ValidationMaxFileSizeMin"].Value ?? "Max file size must be at least 1 MB.");
                return false;
            }
            if (_editModel.IsMultiple && (_editModel.MaxCount < 1 || _editModel.MaxCount > 100))
            {
                await Notify.WarnAsync(L["CreateWizard:ValidationMaxCountRange"].Value ?? "Max files must be between 1 and 100.");
                return false;
            }
            return true;
        }

        if (_wizardStep == 4) // Step 5 Image Processing (only validate when Image type is selected)
        {
            if (!HasFileType(FileType.Image))
                return true;
            if (_editModel.GenerateThumbnail)
            {
                var w = _editModel.ThumbnailWidth;
                var h = _editModel.ThumbnailHeight;
                if (w < 16 || w > 1000 || h < 16 || h > 1000)
                {
                    await Notify.WarnAsync(L["CreateWizard:ValidationThumbnailSize"].Value ?? "Thumbnail size must be between 16 and 1000 px.");
                    return false;
                }
            }
            if (_editModel.EnableWebPConversion)
            {
                var q = _editModel.WebPQuality;
                if (q < 1 || q > 100)
                {
                    await Notify.WarnAsync(L["CreateWizard:ValidationWebPQuality"].Value ?? "WebP quality must be between 1 and 100.");
                    return false;
                }
            }
            return true;
        }

        if (_wizardStep == 5) // Step 6 Storage
        {
            if (_hasDefaultStorageConfigured)
                return true;
            var cfg = _editModel.StorageConfig;
            if (cfg == null) return true;
            if (cfg.StorageProvider == FileStructureStorageProvider.MinIO)
            {
                if (string.IsNullOrWhiteSpace(cfg.MinioEndPoint) || string.IsNullOrWhiteSpace(cfg.MinioBucketName))
                {
                    await Notify.WarnAsync(L["CreateWizard:ValidationMinioRequired"].Value ?? "MinIO EndPoint and Bucket Name are required.");
                    return false;
                }
            }
            if (cfg.StorageProvider == FileStructureStorageProvider.S3Provider)
            {
                if (string.IsNullOrWhiteSpace(cfg.S3Region) || string.IsNullOrWhiteSpace(cfg.S3ContainerName))
                {
                    await Notify.WarnAsync(L["CreateWizard:ValidationS3Required"].Value ?? "S3 Region and Container Name are required.");
                    return false;
                }
            }
            return true;
        }

        return true;
    }

    private async Task SaveStructureFromWizard()
    {
        _saving = true;
        try
        {
            await FileStructureAppService.CreateAsync(_editModel);
            await Notify.SuccessAsync(L["StructureCreatedSuccessfully"]);
            CloseCreateWizard();
            await LoadStructures();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToSaveStructure", ex.Message]);
        }
        finally
        {
            _saving = false;
        }
    }

    private Task OpenEditModal(FileStructureDto structure)
    {
        _isEditing = true;
        _editingId = structure.Id;
        _selectedTab = "basic";
        _showExtensionDetails = false;
        _editModel = new CreateUpdateFileStructureDto
        {
            Key = structure.Key,
            DisplayName = structure.DisplayName,
            Description = structure.Description,
            AllowedFileTypes = structure.AllowedFileTypes,
            AllowedExtensions = structure.AllowedExtensions,
            AllowedMimeTypes = structure.AllowedMimeTypes,
            MaxFileSize = structure.MaxFileSize,
            MinImageWidth = structure.MinImageWidth,
            MinImageHeight = structure.MinImageHeight,
            MaxImageWidth = structure.MaxImageWidth,
            MaxImageHeight = structure.MaxImageHeight,
            IsMultiple = structure.IsMultiple,
            MaxCount = structure.MaxCount,
            IsRequired = structure.IsRequired,
            GenerateThumbnail = structure.GenerateThumbnail,
            ThumbnailWidth = structure.ThumbnailWidth,
            ThumbnailHeight = structure.ThumbnailHeight,
            EnableWebPConversion = structure.EnableWebPConversion,
            WebPQuality = structure.WebPQuality,
            ResizeLargeImages = structure.ResizeLargeImages,
            StorageProvider = structure.StorageProvider,
            IsPublicAccess = structure.IsPublicAccess,
            BaseUrl = structure.BaseUrl,
            StorageConfig = structure.StorageConfig ?? new FileStructureStorageConfigDto { StorageProvider = FileStructureStorageProvider.Database }
        };
        _showDbConnectionString = false;
        _showMinioAccessKey = false;
        _showMinioSecretKey = false;
        _extensionTags = ParseCsvToList(_editModel.AllowedExtensions);
        _mimeTypeTags = ParseCsvToList(_editModel.AllowedMimeTypes);
        _editModalOpen = true;
        return Task.CompletedTask;
    }

    private void CloseEditModal()
    {
        _editModalOpen = false;
    }

    private async Task SaveStructure()
    {
        _saving = true;
        try
        {
            if (_isEditing && _editingId.HasValue)
            {
                await FileStructureAppService.UpdateAsync(_editingId.Value, _editModel);
                await Notify.SuccessAsync(L["StructureUpdatedSuccessfully"]);
            }
            else
            {
                await FileStructureAppService.CreateAsync(_editModel);
                await Notify.SuccessAsync(L["StructureCreatedSuccessfully"]);
            }
            
            CloseEditModal();
            await LoadStructures();
        }
        catch (Exception ex)
        {
            await Notify.ErrorAsync(L["FailedToSaveStructure", ex.Message]);
        }
        finally
        {
            _saving = false;
        }
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

    private void ResetToDefault(FileStructureDto structure)
    {
        _structureToReset = structure;
        _resetConfirmOpen = true;
    }

    private void CancelReset()
    {
        _resetConfirmOpen = false;
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
            await Notify.ErrorAsync(L["FailedToResetStructure", ex.Message]);
        }
        finally
        {
            _resetConfirmOpen = false;
            _structureToReset = null;
        }
    }

    private void DeleteStructure(FileStructureDto structure)
    {
        _structureToDelete = structure;
        _deleteConfirmOpen = true;
    }

    private void CancelDeleteStructure()
    {
        _deleteConfirmOpen = false;
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
            await Notify.ErrorAsync(L["FailedToDeleteStructure", ex.Message]);
        }
        finally
        {
            _deleteConfirmOpen = false;
            _structureToDelete = null;
        }
    }

    private bool HasFileType(FileType type) => _editModel.AllowedFileTypes.HasFlag(type);

    private void ToggleFileType(FileType type, bool enabled)
    {
        if (enabled)
        {
            _editModel.AllowedFileTypes |= type;
        }
        else
        {
            _editModel.AllowedFileTypes &= ~type;
        }
        UpdateExtensionsAndMimeTypes();
    }

    private void UpdateExtensionsAndMimeTypes()
    {
        var extensions = new List<string>();
        var mimeTypes = new List<string>();

        if (_editModel.AllowedFileTypes.HasFlag(FileType.Image))
        {
            extensions.AddRange(new[] { "jpg", "jpeg", "png", "gif", "webp", "bmp", "svg" });
            mimeTypes.AddRange(new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/svg+xml" });
        }

        if (_editModel.AllowedFileTypes.HasFlag(FileType.Video))
        {
            extensions.AddRange(new[] { "mp4", "webm", "mov", "avi", "mkv" });
            mimeTypes.AddRange(new[] { "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo", "video/x-matroska" });
        }

        if (_editModel.AllowedFileTypes.HasFlag(FileType.Document))
        {
            extensions.AddRange(new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt" });
            mimeTypes.AddRange(new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" });
        }

        if (_editModel.AllowedFileTypes.HasFlag(FileType.Audio))
        {
            extensions.AddRange(new[] { "mp3", "wav", "ogg", "flac" });
            mimeTypes.AddRange(new[] { "audio/mpeg", "audio/wav", "audio/ogg", "audio/flac" });
        }

        _editModel.AllowedExtensions = string.Join(",", extensions.Distinct());
        _editModel.AllowedMimeTypes = string.Join(",", mimeTypes.Distinct());
        _extensionTags = ParseCsvToList(_editModel.AllowedExtensions);
        _mimeTypeTags = ParseCsvToList(_editModel.AllowedMimeTypes);
    }

    private static List<string> ParseCsvToList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();
    }

    private void SyncExtensionsToModel(List<string> tags)
    {
        _editModel.AllowedExtensions = string.Join(",", tags);
    }

    private void SyncMimeTypesToModel(List<string> tags)
    {
        _editModel.AllowedMimeTypes = string.Join(",", tags);
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

    private async Task TestStorageConnectionAsync()
    {
        if (_editModel.StorageConfig == null) return;
        _testingConnection = true;
        StateHasChanged();
        try
        {
            var input = new TestStorageConnectionInput
            {
                StorageProvider = _editModel.StorageConfig.StorageProvider,
                DatabaseConnectionString = _editModel.StorageConfig.DatabaseConnectionString,
                FileSystemBasePath = _editModel.StorageConfig.FileSystemBasePath,
                MinioEndPoint = _editModel.StorageConfig.MinioEndPoint,
                MinioAccessKey = _editModel.StorageConfig.MinioAccessKey,
                MinioSecretKey = _editModel.StorageConfig.MinioSecretKey,
                MinioBucketName = _editModel.StorageConfig.MinioBucketName,
                S3EndPoint = _editModel.StorageConfig.S3EndPoint,
                S3Region = _editModel.StorageConfig.S3Region ?? "us-east-1",
                S3AccessKeyId = _editModel.StorageConfig.S3AccessKeyId,
                S3SecretAccessKey = _editModel.StorageConfig.S3SecretAccessKey,
                S3ContainerName = _editModel.StorageConfig.S3ContainerName
            };
            var result = await StorageSettingsAppService.TestConnectionAsync(input);
            if (result.Success)
                await Notify.SuccessAsync(result.Message);
            else
                await Notify.ErrorAsync(result.Message);
        }
        finally
        {
            _testingConnection = false;
            StateHasChanged();
        }
    }

    private string GetStorageProviderLabel(FileStructureStorageProvider provider) =>
        provider switch
        {
            FileStructureStorageProvider.Database => L["StorageProviderDatabase"],
            FileStructureStorageProvider.FileSystem => L["StorageProviderFileSystem"],
            FileStructureStorageProvider.MinIO => L["StorageProviderMinIO"],
            FileStructureStorageProvider.S3Provider => L["StorageProviderS3"],
            _ => provider.ToString()
        };

    private string GetStorageProviderBriefKey(FileStructureStorageProvider provider) =>
        provider switch
        {
            FileStructureStorageProvider.Database => "CreateWizard:StorageProviderBrief:Database",
            FileStructureStorageProvider.FileSystem => "CreateWizard:StorageProviderBrief:FileSystem",
            FileStructureStorageProvider.MinIO => "CreateWizard:StorageProviderBrief:MinIO",
            FileStructureStorageProvider.S3Provider => "CreateWizard:StorageProviderBrief:S3",
            _ => "StorageProvider"
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
