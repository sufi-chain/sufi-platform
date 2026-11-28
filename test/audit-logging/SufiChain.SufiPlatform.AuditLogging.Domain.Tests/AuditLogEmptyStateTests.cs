using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.AuditLogging;

public class AuditLogEmptyStateTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("fa")]
    [InlineData("ar")]
    public void Empty_state_keys_exist(string culture)
    {
        var json = File.ReadAllText(Find($"Localization/AuditLogging/{culture}.json"));
        foreach (var key in new[]
        {
            "NoActionsRecorded",
            "NoEntityChangesRecorded",
            "NoExceptions",
            "SelectAuditLogToViewDetails"
        })
        {
            json.ShouldContain($"\"{key}\"");
        }
    }

    [Fact]
    public void Detail_modal_does_not_use_the_english_empty_title()
    {
        var razor = File.ReadAllText(Find(
            "SufiChain.SufiPlatform.AuditLogging.Blazor/Components/AuditLogDetailModal.razor",
            "modules/audit-logging/src"));
        razor.ShouldNotContain("No items found");
        razor.Split("SbEmptyState Title=\"\"").Length.ShouldBe(5);
    }

    private static string Find(string relative, string anchor = "modules/audit-logging/src/SufiChain.SufiPlatform.AuditLogging.Domain.Shared")
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(dir.FullName, anchor, relative);
            if (File.Exists(path))
            {
                return path;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relative);
    }
}
