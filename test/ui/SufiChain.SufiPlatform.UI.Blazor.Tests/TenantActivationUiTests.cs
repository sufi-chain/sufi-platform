using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.Blazor.Components;
using SufiChain.SufiPlatform.UI.Messages;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class TenantActivationUiTests : BunitContext
{
    [Fact]
    public async Task EditTenant_ShowsActiveSwitch()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        localizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        var blazorLocalizer = Substitute.For<IStringLocalizer<SufiBlazorResource>>();
        blazorLocalizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
        blazorLocalizer[Arg.Any<string>(), Arg.Any<object[]>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        var factory = Substitute.For<IStringLocalizerFactory>();
        factory.Create(Arg.Any<Type>()).Returns(localizer);
        factory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(localizer);
        var messages = Substitute.For<IUiMessageService>();
        messages.ConfirmAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<Action<UiMessageOptions>?>())
            .Returns(true);
        var tenants = Substitute.For<ITenantAppService>();
        tenants.DeactivateAsync(Arg.Any<Guid>()).Returns(Task.CompletedTask);
        Services.AddSingleton(factory);
        Services.AddSingleton(blazorLocalizer);
        Services.AddSingleton(messages);
        Services.AddSingleton(tenants);

        var tenantId = Guid.Parse("21100000-0000-0000-0000-000000000211");
        var cut = Render<TenantEditModal>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.Tenant, new TenantDto
            {
                Id = tenantId,
                Name = "Demo",
                IsActive = true,
                ConcurrencyStamp = "stamp"
            }));

        var toggle = cut.Find("input[role=switch]");
        toggle.GetAttribute("aria-checked").ShouldBe("true");
        cut.Markup.ShouldContain("IsActive");
        await cut.InvokeAsync(() => toggle.Change(false));

        await tenants.Received(1).DeactivateAsync(tenantId);
        await messages.Received(1).ConfirmAsync(
            Arg.Is<string>(text => text.Contains("DeactivateTenantConfirmation")),
            Arg.Any<string?>(),
            Arg.Any<Action<UiMessageOptions>?>());
    }
}
