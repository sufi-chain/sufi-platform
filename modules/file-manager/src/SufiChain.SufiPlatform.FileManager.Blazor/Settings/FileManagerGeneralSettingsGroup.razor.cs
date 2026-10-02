using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Settings;

public partial class FileManagerGeneralSettingsGroup : FileManagerComponentBase, ISaveableSettingGroup
{
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
    }, LoadingKeys.Load);

    public Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _settings.MaxFileSizeBytes = Math.Max(1, _maxFileSizeMegabytes) * BytesPerMegabyte;
        _settings.AllowedImageExtensions = JoinList(_imageExtensions);
        _settings.AllowedVideoExtensions = JoinList(_videoExtensions);
        _settings.AllowedDocumentExtensions = JoinList(_documentExtensions);
        await SettingsAppService.UpdateGeneralSettingsAsync(_settings);
        await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
    }, LoadingKeys.Save);

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
