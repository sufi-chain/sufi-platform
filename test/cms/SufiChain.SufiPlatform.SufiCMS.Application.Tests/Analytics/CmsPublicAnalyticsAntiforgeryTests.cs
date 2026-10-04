using System.Reflection;
using Shouldly;
using SufiChain.SufiPlatform.SufiCMS.Analytics;
using SufiChain.SufiPlatform.SufiCMS.Controllers;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCMS.Analytics;

public class CmsPublicAnalyticsAntiforgeryTests
{
    [Fact]
    public void ApplyScriptAttribute_sets_the_request_token_and_skips_a_blank_token()
    {
        var attributes = new Dictionary<string, string?>();

        CmsPublicAnalyticsAntiforgery.ApplyScriptAttribute(attributes, "token-value");
        attributes[CmsPublicAnalyticsAntiforgery.ScriptAttributeName].ShouldBe("token-value");

        attributes.Clear();
        CmsPublicAnalyticsAntiforgery.ApplyScriptAttribute(attributes, "  ");
        attributes.ShouldBeEmpty();
    }

    [Fact]
    public void Analytics_script_sends_the_page_token_before_the_cookie()
    {
        var script = File.ReadAllText(FindWorkspaceFile(
            "pro-modules/cms/src/SufiChain.SufiPlatform.SufiCMS.Public.Blazor/wwwroot/cms-analytics.js"));
        var attribute = script.IndexOf(CmsPublicAnalyticsAntiforgery.ScriptAttributeName, StringComparison.Ordinal);
        var cookie = script.IndexOf("XSRF-TOKEN", StringComparison.Ordinal);
        var header = script.IndexOf(CmsPublicAnalyticsAntiforgery.HeaderName, StringComparison.Ordinal);

        attribute.ShouldBeGreaterThanOrEqualTo(0);
        cookie.ShouldBeGreaterThan(attribute);
        header.ShouldBeGreaterThan(cookie);
    }

    [Fact]
    public void Capture_endpoint_ignores_antiforgery_for_anonymous_view_capture()
    {
        var method = typeof(PublicAnalyticsController).GetMethod(nameof(PublicAnalyticsController.CaptureAsync));
        method.ShouldNotBeNull();

        // DEBT-078: a stale XSRF or Identity cookie made ABP reject this anonymous POST.
        // The page still sends RequestVerificationToken when it has one.
        var ignored = method!.GetCustomAttributes(inherit: true)
            .Concat(typeof(PublicAnalyticsController).GetCustomAttributes(inherit: true))
            .Any(attribute => attribute.GetType().Name == "IgnoreAntiforgeryTokenAttribute");

        ignored.ShouldBeTrue();
    }

    private static string FindWorkspaceFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
