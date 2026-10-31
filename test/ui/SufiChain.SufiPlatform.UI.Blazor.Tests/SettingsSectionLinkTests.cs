using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SettingsSectionLinkTests
{
    [Fact]
    public void Section_links_keep_the_tab_and_drop_a_replaced_section()
    {
        SettingsSectionLink.For("communications", "sms")
            .ShouldBe("/panel/admin/settings?section=communications&tab=sms");

        var redirected = SettingsSectionLink.Redirect(
            "https://localhost/panel/admin/communications/providers/email-config?culture=fa&section=old&tab=voice&returnUrl=%2Fpanel",
            "communications",
            "email");

        redirected.ShouldBe("/panel/admin/settings?section=communications&tab=email&culture=fa&returnUrl=%2Fpanel");
        SettingsSectionLink.ReadTab("https://localhost/panel/admin/settings?section=communications&tab=voice")
            .ShouldBe("voice");
    }
}
