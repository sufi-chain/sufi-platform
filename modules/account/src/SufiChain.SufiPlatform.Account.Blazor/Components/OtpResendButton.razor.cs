using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Components.Actions;
using SufiChain.SufiPlatform.Account.Localization;

namespace SufiChain.SufiPlatform.Account.Blazor.Components;

/// <summary>
/// Resend button that stays disabled and shows an mm:ss countdown until <see cref="AvailableAt"/>.
/// </summary>
public partial class OtpResendButton : ComponentBase, IDisposable
{
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _timerCts;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Parameter]
    public DateTimeOffset? AvailableAt { get; set; }

    [Parameter]
    public EventCallback OnResend { get; set; }

    [Parameter]
    public bool Loading { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public SbColor Color { get; set; } = SbColor.Primary;

    [Parameter]
    public SbButtonVariant Variant { get; set; } = SbButtonVariant.Link;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected int RemainingSeconds { get; private set; }

    protected override void OnParametersSet()
    {
        RemainingSeconds = ComputeRemainingSeconds();
        if (RemainingSeconds > 0 && _timer == null)
        {
            _ = RunCountdownAsync();
        }
    }

    public static string FormatRemaining(int seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return time.TotalHours >= 1
            ? ((int)time.TotalHours).ToString(CultureInfo.InvariantCulture) + time.ToString(@"\:mm\:ss", CultureInfo.InvariantCulture)
            : time.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    protected virtual int ComputeRemainingSeconds()
    {
        if (!AvailableAt.HasValue)
        {
            return 0;
        }

        var remaining = AvailableAt.Value - DateTimeOffset.UtcNow;
        return remaining <= TimeSpan.Zero ? 0 : (int)Math.Ceiling(remaining.TotalSeconds);
    }

    private async Task RunCountdownAsync()
    {
        _timerCts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var token = _timerCts.Token;

        try
        {
            while (await _timer.WaitForNextTickAsync(token))
            {
                RemainingSeconds = ComputeRemainingSeconds();
                await InvokeAsync(StateHasChanged);
                if (RemainingSeconds <= 0)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            StopTimer();
        }
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
        var timerCts = Interlocked.Exchange(ref _timerCts, null);
        if (timerCts is not null)
        {
            try
            {
                timerCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                timerCts.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    public void Dispose()
    {
        StopTimer();
        GC.SuppressFinalize(this);
    }
}
