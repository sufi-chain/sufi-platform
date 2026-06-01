using Microsoft.AspNetCore.Components;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatComposer : ChatPublicComponentBase
{
    [Parameter]
    public string DraftMessage { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> DraftMessageChanged { get; set; }

    [Parameter]
    public EventCallback<string> OnSend { get; set; }

    [Parameter]
    public bool IsDisabled { get; set; }

    [Parameter]
    public bool IsSignupRequired { get; set; }

    [Parameter]
    public string? SignupRequiredLocalizationKey { get; set; }

    [Parameter]
    public string? SignInUrl { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ComposerToolbar { get; set; }

    private string _draft = string.Empty;

    protected override void OnParametersSet()
    {
        _draft = DraftMessage;
    }

    protected string GetSignupRequiredMessage()
    {
        var key = string.IsNullOrWhiteSpace(SignupRequiredLocalizationKey)
            ? "Chat:AuthenticationRequired"
            : SignupRequiredLocalizationKey;

        return L[key];
    }

    protected async Task SendAsync()
    {
        if (IsDisabled || IsSignupRequired || string.IsNullOrWhiteSpace(_draft))
        {
            return;
        }

        var message = _draft.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        await OnSend.InvokeAsync(message);
        _draft = string.Empty;
        await DraftMessageChanged.InvokeAsync(_draft);
    }
}
