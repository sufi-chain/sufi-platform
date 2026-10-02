using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Pages;

public partial class FileStructureWizard : FileManagerComponentBase
{
    public const string ListRoute = "/panel/admin/file-manager/structures";

    private const int WizardStepCount = 7;

    private static readonly FileStructureStorageProvider[] _storageProviderOptions =
    {
        FileStructureStorageProvider.Database,
        FileStructureStorageProvider.FileSystem,
        FileStructureStorageProvider.MinIO,
        FileStructureStorageProvider.S3Provider
    };

    [Parameter] public Guid? StructureId { get; set; }

    [Inject] private IFileStructureAppService FileStructureAppService { get; set; } = default!;
    [Inject] private IFileManagerStorageSettingsAppService StorageSettingsAppService { get; set; } = default!;
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    private bool _loading = true;
    private bool _ready;
    private bool _notFound;
    private bool _saving;
    private bool _testingConnection;
    private int _wizardStep;
    private string? _loadKey;
    private EditContext? _editContext;
    private CreateUpdateFileStructureDto _editModel = new();
    private List<string> _extensionTags = new();
    private List<string> _mimeTypeTags = new();
    private bool _showDbConnectionString;
    private bool _showMinioAccessKey;
    private bool _showMinioSecretKey;
    private bool _showS3AccessKey;
    private bool _showS3SecretKey;
    private FileStructureStorageConfigDto? _defaultStorageConfig;
    private bool _hasDefaultStorageConfigured;

    private bool IsCreate => !StructureId.HasValue;

    protected override async Task OnParametersSetAsync()
    {
        var key = IsCreate ? "new" : StructureId!.Value.ToString("N");
        if (key == _loadKey)
        {
            return;
        }

        _loadKey = key;
        PageLayout.Title = IsCreate ? L["CreateStructure"] : L["EditStructure"];
        _wizardStep = 0;
        _notFound = false;
        _ready = false;

        if (IsCreate)
        {
            InitializeCreateModel();
            _loading = false;
            _ready = true;
            return;
        }

        await LoadForEditAsync();
    }

    private void InitializeCreateModel()
    {
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
        ResetSecretEditors();
        _extensionTags = ParseCsvToList(_editModel.AllowedExtensions);
        _mimeTypeTags = ParseCsvToList(_editModel.AllowedMimeTypes);
        _defaultStorageConfig = null;
        _hasDefaultStorageConfigured = false;
        _editContext = new EditContext(_editModel);
    }

    private async Task LoadForEditAsync()
    {
        _loading = true;
        try
        {
            var structure = await FileStructureAppService.GetAsync(StructureId!.Value);
            if (IsDisposed)
            {
                return;
            }

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
            ResetSecretEditors();
            _extensionTags = ParseCsvToList(_editModel.AllowedExtensions);
            _mimeTypeTags = ParseCsvToList(_editModel.AllowedMimeTypes);
            _defaultStorageConfig = null;
            _hasDefaultStorageConfigured = false;
            _editContext = new EditContext(_editModel);
            _ready = true;
        }
        catch (Exception ex)
        {
            if (IsDisposed)
            {
                return;
            }

            _notFound = true;
            _ready = false;
            await NotifyOperationFailedAsync(ex, "FailedToLoadStructures");
        }
        finally
        {
            _loading = false;
        }
    }

    private void ResetSecretEditors()
    {
        _showDbConnectionString = false;
        _showMinioAccessKey = false;
        _showMinioSecretKey = false;
        _showS3AccessKey = false;
        _showS3SecretKey = false;
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo(ListRoute);
    }

    private async Task WizardNext()
    {
        if (!await ValidateCurrentStepAsync())
        {
            return;
        }

        _wizardStep = Math.Min(_wizardStep + 1, WizardStepCount - 1);
        if (_wizardStep == 5 && IsCreate)
        {
            await LoadDefaultStorageConfigAsync();
        }
    }

    private void WizardBack()
    {
        _wizardStep = Math.Max(0, _wizardStep - 1);
    }

    private async Task WizardFinish()
    {
        if (_editContext == null || !_editContext.Validate())
        {
            return;
        }

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
        if (config == null)
        {
            return false;
        }

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
        if (_editContext == null)
        {
            return true;
        }

        if (_wizardStep == 1)
        {
            _editContext.Validate();
            if (_editContext.GetValidationMessages().Any())
            {
                return false;
            }

            return true;
        }

        if (_wizardStep == 2)
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

        if (_wizardStep == 3)
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

        if (_wizardStep == 4)
        {
            if (!HasFileType(FileType.Image))
            {
                return true;
            }

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

        if (_wizardStep == 5)
        {
            if (_hasDefaultStorageConfigured)
            {
                return true;
            }

            var cfg = _editModel.StorageConfig;
            if (cfg == null)
            {
                return true;
            }

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
            if (IsCreate)
            {
                await FileStructureAppService.CreateAsync(_editModel);
                await Notify.SuccessAsync(L["StructureCreatedSuccessfully"]);
            }
            else
            {
                await FileStructureAppService.UpdateAsync(StructureId!.Value, _editModel);
                await Notify.SuccessAsync(L["StructureUpdatedSuccessfully"]);
            }

            NavigationManager.NavigateTo(ListRoute);
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                await NotifyOperationFailedAsync(ex, "FailedToSaveStructure");
            }
        }
        finally
        {
            _saving = false;
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
        if (string.IsNullOrWhiteSpace(csv))
        {
            return new List<string>();
        }

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

    private async Task TestStorageConnectionAsync()
    {
        if (_editModel.StorageConfig == null)
        {
            return;
        }

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
            {
                await Notify.SuccessAsync(result.Message);
            }
            else
            {
                await Notify.ErrorAsync(result.Message);
            }
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

    private List<string> GetFileTypes(FileType types)
    {
        var result = new List<string>();
        if (types.HasFlag(FileType.Image))
        {
            result.Add("FileTypeImage");
        }

        if (types.HasFlag(FileType.Video))
        {
            result.Add("FileTypeVideo");
        }

        if (types.HasFlag(FileType.Document))
        {
            result.Add("FileTypeDocument");
        }

        if (types.HasFlag(FileType.Audio))
        {
            result.Add("FileTypeAudio");
        }

        return result;
    }

    private string FormatSize(long bytes)
    {
        if (bytes == 0)
        {
            return "0 B";
        }

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
}
