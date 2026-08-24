using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Captcha;

namespace SufiChain.SufiPlatform.Account.Blazor.Components;

public partial class SufiCaptcha : ComponentBase, IAsyncDisposable
{
    private const string TurnstileScriptUrl = "https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit";
    private const string RecaptchaScriptUrl = "https://www.google.com/recaptcha/api.js?render=explicit";

    private readonly string _turnstileElementId = $"turnstile-{Guid.NewGuid():N}";
    private readonly string _recaptchaElementId = $"recaptcha-{Guid.NewGuid():N}";
    private DotNetObjectReference<SufiCaptcha>? _dotNetRef;
    private bool _externalWidgetRendered;
    private int _lastResetVersion;

    [Inject]
    protected ICaptchaAppService CaptchaAppService { get; set; } = default!;

    [Inject]
    protected IStringLocalizer<SufiAccountResource> AccountL { get; set; } = default!;

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter]
    public string? CaptchaChallengeId { get; set; }

    [Parameter]
    public EventCallback<string?> CaptchaChallengeIdChanged { get; set; }

    [Parameter]
    public string? CaptchaAnswer { get; set; }

    [Parameter]
    public EventCallback<string?> CaptchaAnswerChanged { get; set; }

    [Parameter]
    public string? CaptchaToken { get; set; }

    [Parameter]
    public EventCallback<string?> CaptchaTokenChanged { get; set; }

    [Parameter]
    public int ResetVersion { get; set; }

    protected CaptchaOptionsDto? Options { get; set; }

    protected CaptchaChallengeDto? Challenge { get; set; }

    protected bool IsLoading { get; set; }

    protected string? LoadError { get; set; }

    protected bool IsVisible => Options?.IsEnabled == true;

    protected bool IsSimpleProvider =>
        string.Equals(Options?.Provider, CaptchaProviderNames.Simple, StringComparison.OrdinalIgnoreCase);

    protected bool IsTurnstileProvider =>
        string.Equals(Options?.Provider, CaptchaProviderNames.Turnstile, StringComparison.OrdinalIgnoreCase);

    protected bool IsRecaptchaProvider =>
        string.Equals(Options?.Provider, CaptchaProviderNames.Recaptcha, StringComparison.OrdinalIgnoreCase);

    protected string CaptchaAnswerValue { get; set; } = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        CaptchaAnswerValue = CaptchaAnswer ?? string.Empty;
        if (ResetVersion != _lastResetVersion)
        {
            _lastResetVersion = ResetVersion;
            await ResetAsync();
        }

        await base.OnParametersSetAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!IsVisible || IsLoading || !string.IsNullOrEmpty(LoadError) || _externalWidgetRendered)
        {
            return;
        }

        if (IsTurnstileProvider && !string.IsNullOrWhiteSpace(Options?.SiteKey))
        {
            await RenderTurnstileAsync();
        }
        else if (IsRecaptchaProvider && !string.IsNullOrWhiteSpace(Options?.SiteKey))
        {
            await RenderRecaptchaAsync();
        }
    }

    protected virtual async Task LoadAsync()
    {
        IsLoading = true;
        LoadError = null;

        try
        {
            Options = await CaptchaAppService.GetOptionsAsync();
            if (Options.IsEnabled && IsSimpleProvider)
            {
                await RefreshChallengeAsync();
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task RefreshChallengeAsync()
    {
        Challenge = await CaptchaAppService.GetChallengeAsync();
        CaptchaChallengeId = Challenge.ChallengeId;
        CaptchaAnswer = null;
        CaptchaAnswerValue = string.Empty;

        await CaptchaChallengeIdChanged.InvokeAsync(CaptchaChallengeId);
        await CaptchaAnswerChanged.InvokeAsync(CaptchaAnswer);
    }

    protected virtual async Task PublishCaptchaAnswerAsync()
    {
        CaptchaAnswer = string.IsNullOrEmpty(CaptchaAnswerValue) ? null : CaptchaAnswerValue;
        await CaptchaAnswerChanged.InvokeAsync(CaptchaAnswer);
    }

    protected virtual async Task OnRefreshClickAsync()
    {
        await ResetAsync();
    }

    public virtual async Task ResetAsync()
    {
        CaptchaAnswer = null;
        CaptchaAnswerValue = string.Empty;
        CaptchaToken = null;

        await CaptchaAnswerChanged.InvokeAsync(null);
        await CaptchaTokenChanged.InvokeAsync(null);

        if (IsSimpleProvider && Options?.IsEnabled == true)
        {
            await RefreshChallengeAsync();
            return;
        }

        if (_externalWidgetRendered)
        {
            try
            {
                if (IsTurnstileProvider)
                {
                    await JsRuntime.InvokeVoidAsync("sufiAbpCaptcha.resetTurnstile");
                }
                else if (IsRecaptchaProvider)
                {
                    await JsRuntime.InvokeVoidAsync("sufiAbpCaptcha.resetRecaptcha");
                }
            }
            catch (JSException)
            {
                // The widget may not be available during static SSR or teardown.
            }
        }
    }

    [JSInvokable]
    public async Task OnExternalCaptchaTokenAsync(string? token)
    {
        CaptchaToken = token;
        await CaptchaTokenChanged.InvokeAsync(CaptchaToken);
    }

    protected virtual async Task RenderTurnstileAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("sufiAbpCaptcha.loadScript", TurnstileScriptUrl);
            var rendered = await JsRuntime.InvokeAsync<bool>(
                "sufiAbpCaptcha.renderTurnstile",
                _turnstileElementId,
                Options!.SiteKey,
                _dotNetRef);

            if (!rendered)
            {
                LoadError = AccountL["CaptchaExternalProviderNotConfigured"];
            }
            else
            {
                _externalWidgetRendered = true;
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
    }

    protected virtual async Task RenderRecaptchaAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("sufiAbpCaptcha.loadScript", RecaptchaScriptUrl);
            var rendered = await JsRuntime.InvokeAsync<bool>(
                "sufiAbpCaptcha.renderRecaptcha",
                _recaptchaElementId,
                Options!.SiteKey,
                _dotNetRef);

            if (!rendered)
            {
                LoadError = AccountL["CaptchaExternalProviderNotConfigured"];
            }
            else
            {
                _externalWidgetRendered = true;
            }
        }
        catch (Exception ex)
        {
            LoadError = ex.Message;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        await Task.CompletedTask;
    }
}
