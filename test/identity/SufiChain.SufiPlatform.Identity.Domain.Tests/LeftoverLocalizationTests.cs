using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class LeftoverLocalizationTests
{
    [Fact]
    public void TagCreateModal_Fa_CancelIsLocalized()
    {
        var fa = File.ReadAllText(Find("modules/tags/src/SufiChain.SufiPlatform.Tags.Domain.Shared/Localization/Tags/fa.json"));
        fa.ShouldContain("\"Cancel\": \"انصراف\"");
    }

    [Fact]
    public void CacheManagementGroupAndItems_HaveDisplayNames_EnFa()
    {
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var json = File.ReadAllText(Find(
                $"framework/SufiChain.SufiPlatform.Ddd.Application.Contracts/Localization/SufiDddApplicationContracts/{culture}.json"));
            json.ShouldContain("\"Permission:CacheManagement\":");
            json.ShouldContain("\"Permission:CacheManagement.Manage\":");
            json.ShouldNotContain("\"Permission:CacheManagement\": \"Permission:");
        }
    }

    [Fact]
    public void RoleManagement_Fa_DefaultBadgeIsLocalized()
    {
        var fa = File.ReadAllText(Find("modules/identity/src/SufiChain.SufiPlatform.Identity.Domain.Shared/Localization/Identity/fa.json"));
        fa.ShouldContain("\"Default\": \"پیش‌فرض\"");
    }

    private static string Find(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(relative);
    }
}
