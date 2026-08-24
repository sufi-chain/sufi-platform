using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components;

/// <summary>
/// In-browser audio player for public file display. Uses the stream URL when available.
/// </summary>
public partial class FileAudio : ComponentBase
{
    [Inject]
    protected IFilePublicUrlResolver UrlResolver { get; set; } = default!;

    [Parameter]
    public Guid FileId { get; set; }

    /// <summary>
    /// Play even when File Manager metadata is not marked as audio
    /// (composer voice files, missing MIME).
    /// </summary>
    [Parameter]
    public bool ForceAudio { get; set; }

    [Parameter]
    public bool Controls { get; set; } = true;

    [Parameter]
    public string Preload { get; set; } = "metadata";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    [Parameter]
    public RenderFragment? ErrorTemplate { get; set; }

    private string? _playerUrl;
    private bool _isLoading = true;
    private bool _hasError;
    private Guid _lastFileId;
    private bool _lastForceAudio;

    protected override async Task OnParametersSetAsync()
    {
        if (FileId != Guid.Empty && (FileId != _lastFileId || ForceAudio != _lastForceAudio))
        {
            _lastFileId = FileId;
            _lastForceAudio = ForceAudio;
            await LoadAudioInfoAsync();
        }
    }

    private async Task LoadAudioInfoAsync()
    {
        _isLoading = true;
        _hasError = false;
        _playerUrl = null;
        StateHasChanged();

        try
        {
            var fileInfo = await UrlResolver.GetFilePublicInfoAsync(FileId);
            var canPlay = ForceAudio ||
                          fileInfo?.IsPlayableAudio == true ||
                          FilePublicInfo.IsComposerVoiceFileName(fileInfo?.FileName);

            if (fileInfo == null || !canPlay)
            {
                _hasError = true;
                return;
            }

            _playerUrl = !string.IsNullOrWhiteSpace(fileInfo.StreamUrl)
                ? fileInfo.StreamUrl
                : fileInfo.DownloadUrl;

            if (string.IsNullOrWhiteSpace(_playerUrl))
            {
                _playerUrl = await UrlResolver.GetStreamUrlAsync(FileId);
            }

            if (string.IsNullOrWhiteSpace(_playerUrl))
            {
                _hasError = true;
            }
        }
        catch
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
