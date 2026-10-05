using System.Globalization;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiBlazor.Components.Forms;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class QuietSettingsSaveTests : BunitContext
{
    private readonly RecordingExceptionInformer _informer = new();

    public QuietSettingsSaveTests()
    {
        Services.AddSingleton<IUserExceptionInformer>(_informer);
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new QuietSaveLocalizer());
    }

    [Fact]
    public void ThrowingSave_ReturnsFalse_AndNeverCallsUserExceptionInformer()
    {
        var cut = Render<QuietSaveHost>(parameters => parameters.Add(component => component.Action, () =>
            Task.FromException(new InvalidOperationException("save failed"))));

        cut.WaitForAssertion(() => Assert.False(cut.Instance.Saved));
        Assert.Equal(0, _informer.Calls);
        Assert.DoesNotContain("sb-dialog", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidationFailure_MarksTheField_AndNeverCallsUserExceptionInformer()
    {
        var cut = Render<QuietSaveHost>(parameters => parameters.Add(component => component.Action, () =>
            Task.FromException(new AbpValidationException())));

        cut.WaitForAssertion(() =>
        {
            Assert.False(cut.Instance.Saved);
            Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        });
        Assert.Equal(0, _informer.Calls);
    }

    private sealed class RecordingExceptionInformer : IUserExceptionInformer
    {
        public int Calls { get; private set; }

        public void Inform(UserExceptionInformerContext context) => Calls++;

        public Task InformAsync(UserExceptionInformerContext context)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    private sealed class QuietSaveLocalizer : IStringLocalizer<SufiBlazorResource>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}

sealed class AbpValidationException : Exception
{
    public IReadOnlyList<MemberError> ValidationErrors { get; } =
        [new MemberError(["SmtpHost"])];

    public sealed class MemberError
    {
        public MemberError(IEnumerable<string> memberNames) => MemberNames = memberNames;

        public IEnumerable<string> MemberNames { get; }
    }
}

file sealed class QuietSaveHost : SufiComponentBase
{
    private bool _started;

    [Parameter]
    public Func<Task>? Action { get; set; }

    public bool? Saved { get; private set; }

    public string SmtpHost { get; set; } = "mail.example";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SbSaveFieldErrorSet>>(0);
        builder.AddAttribute(1, "Value", SaveFieldErrors);
        builder.AddAttribute(2, "IsFixed", false);
        builder.AddAttribute(3, "ChildContent", (RenderFragment)(child =>
        {
            child.OpenComponent<SbTextField<string>>(0);
            child.AddAttribute(1, "Value", SmtpHost);
            child.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(this, value => SmtpHost = value));
            child.AddAttribute(3, "ValueExpression", (Expression<Func<string>>)(() => SmtpHost));
            child.CloseComponent();
        }));
        builder.CloseComponent();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _started)
        {
            return;
        }

        _started = true;
        Saved = await TrySaveQuietlyAsync(() => Action?.Invoke() ?? Task.CompletedTask, "save");
    }
}
