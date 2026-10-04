using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerGeneralSettingsGroup : FileManagerComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<GeneralEdit> _edits;

    public FileManagerGeneralSettingsGroup()
    {
        _edits = new SettingGroupEditor<GeneralEdit>(ReadEdit, WriteEdit);
    }
    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    [Inject] private IFileManagerSettingsAppService SettingsAppService { get; set; } = default!;

    private const long BytesPerMegabyte = 1024 * 1024;

    private FileManagerGeneralSettingsDto _settings = new();
    private long _maxFileSizeMegabytes = 1;
    private List<string> _imageExtensions = [];
    private List<string> _videoExtensions = [];
    private List<string> _documentExtensions = [];

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _edits.Observe();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await LoadAsync();
        }
    }

    private Task LoadAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _settings = await SettingsAppService.GetGeneralSettingsAsync();
        _maxFileSizeMegabytes = Math.Max(1, _settings.MaxFileSizeBytes / BytesPerMegabyte);
        _imageExtensions = SplitList(_settings.AllowedImageExtensions);
        _videoExtensions = SplitList(_settings.AllowedVideoExtensions);
        _documentExtensions = SplitList(_settings.AllowedDocumentExtensions);
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        if (await TrySaveAsync())
        {
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }
    }

    public async Task<bool> TrySaveAsync()
    {
        var saved = await ExecuteWithLoadingAsync(async () =>
        {
            _settings.MaxFileSizeBytes = Math.Max(1, _maxFileSizeMegabytes) * BytesPerMegabyte;
            _settings.AllowedImageExtensions = JoinList(_imageExtensions);
            _settings.AllowedVideoExtensions = JoinList(_videoExtensions);
            _settings.AllowedDocumentExtensions = JoinList(_documentExtensions);
            await SettingsAppService.UpdateGeneralSettingsAsync(_settings);
            _edits.Capture();
            return true;
        }, LoadingKeys.Save);

        return saved == true;
    }

    public Task DiscardAsync()
    {
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

    private GeneralEdit ReadEdit() => new()
    {
        Settings = _settings,
        MaxFileSizeMegabytes = _maxFileSizeMegabytes,
        ImageExtensions = _imageExtensions.ToList(),
        VideoExtensions = _videoExtensions.ToList(),
        DocumentExtensions = _documentExtensions.ToList()
    };

    private void WriteEdit(GeneralEdit? value)
    {
        if (value == null)
        {
            return;
        }

        _settings = value.Settings ?? new FileManagerGeneralSettingsDto();
        _maxFileSizeMegabytes = value.MaxFileSizeMegabytes;
        _imageExtensions = value.ImageExtensions ?? [];
        _videoExtensions = value.VideoExtensions ?? [];
        _documentExtensions = value.DocumentExtensions ?? [];
    }

    private sealed class GeneralEdit
    {
        public FileManagerGeneralSettingsDto Settings { get; set; } = new();

        public long MaxFileSizeMegabytes { get; set; }

        public List<string> ImageExtensions { get; set; } = [];

        public List<string> VideoExtensions { get; set; } = [];

        public List<string> DocumentExtensions { get; set; } = [];
    }

    private void OnImageExtensionsChanged(List<string> tags) => _imageExtensions = tags;

    private void OnVideoExtensionsChanged(List<string> tags) => _videoExtensions = tags;

    private void OnDocumentExtensionsChanged(List<string> tags) => _documentExtensions = tags;

    private static List<string> SplitList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split([',', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string JoinList(IEnumerable<string> tags) =>
        string.Join(",", tags.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).Distinct(StringComparer.OrdinalIgnoreCase));
}
