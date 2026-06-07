using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposerVoiceButton : ChatPublicComponentBase
{
    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public int MaxSeconds { get; set; } = 120;

    [Parameter]
    public bool IsRecording { get; set; }

    [Parameter]
    public EventCallback OnRecordingStarted { get; set; }

    [Parameter]
    public EventCallback<int> OnRecordingStopped { get; set; }

    private Stopwatch? _stopwatch;

    protected async Task StartAsync()
    {
        Logger.LogDebug("[ChatComposer] VoiceButton StartAsync (pointerdown) — IsDisabled={Disabled}, IsRecording={Recording}", IsDisabled, IsRecording);

        if (IsDisabled || IsRecording)
        {
            Logger.LogDebug("[ChatComposer] VoiceButton StartAsync ignored — disabled or already recording");
            return;
        }

        _stopwatch = Stopwatch.StartNew();
        await OnRecordingStarted.InvokeAsync();
    }

    protected async Task StopAsync()
    {
        Logger.LogDebug("[ChatComposer] VoiceButton StopAsync (pointerup/leave) — IsRecording={Recording}, hasStopwatch={HasSw}", IsRecording, _stopwatch != null);

        if (!IsRecording || _stopwatch == null)
        {
            Logger.LogDebug("[ChatComposer] VoiceButton StopAsync ignored — not recording");
            return;
        }

        _stopwatch.Stop();
        var seconds = Math.Max(1, (int)Math.Ceiling(_stopwatch.Elapsed.TotalSeconds));
        _stopwatch = null;
        await OnRecordingStopped.InvokeAsync(Math.Min(seconds, MaxSeconds));
    }
}
