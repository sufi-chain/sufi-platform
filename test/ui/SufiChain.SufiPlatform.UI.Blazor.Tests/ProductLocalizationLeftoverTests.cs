using System.Text.Json;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

/// <summary>
/// SMK-05: fa, en, ar, and es must all carry these product strings, and ar must not keep the English
/// or Persian fallback.
/// </summary>
public class ProductLocalizationLeftoverTests
{
    [Fact]
    public void Portal_ticket_and_posts_labels_exist_in_fa_en_ar_and_es()
    {
        var portal = Load("hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Domain.Shared/Localization/SufiPlatform");
        AssertTranslated(portal, "Menu:Portal");
        portal["ar"]["Menu:Portal"].ShouldBe("البوابة");
        portal["ar"]["Menu:Portal"].ShouldNotBe("پورتال");

        var tickets = Load("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Domain.Shared/Localization/Ticketing");
        AssertTranslated(tickets, "Ticketing:CreateTicket");
        AssertTranslated(tickets, "Ticketing:TicketNumber");
        tickets["ar"]["Ticketing:CreateTicket"].ShouldBe("إنشاء تذكرة");
        tickets["ar"]["Ticketing:TicketNumber"].ShouldBe("رقم التذكرة");

        var posts = Load("pro-modules/cms/src/SufiChain.SufiPlatform.SufiCMS.Domain.Shared/Localization/Cms");
        AssertTranslated(posts, "Block:BlogList:Label");
        AssertTranslated(posts, "Block:BlogList:Empty");
        posts["fa"]["Block:BlogList:Label"].ShouldNotBe(posts["en"]["Block:BlogList:Label"]);
        posts["ar"]["Block:BlogList:Label"].ShouldNotBe(posts["en"]["Block:BlogList:Label"]);
        posts["fa"]["Block:BlogList:Empty"].ShouldNotBe("No posts yet.");
        posts["ar"]["Block:BlogList:Empty"].ShouldNotBe("No posts yet.");
    }

    [Fact]
    public void New_folder_default_uses_the_localized_name()
    {
        var razor = File.ReadAllText(Find(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Blazor/Components/FileManager/FileManager.razor"));
        razor.ShouldContain("_newFolderName = L[\"NewFolder\"]");
        razor.ShouldNotContain("_newFolderName = \"New Folder\"");
    }

    private static void AssertTranslated(Dictionary<string, Dictionary<string, string>> cultures, string key)
    {
        foreach (var culture in new[] { "fa", "en", "ar", "es" })
        {
            cultures[culture].ContainsKey(key).ShouldBeTrue($"{culture} missing {key}");
            cultures[culture][key].ShouldNotBeNullOrWhiteSpace($"{culture} {key}");
        }
    }

    private static Dictionary<string, Dictionary<string, string>> Load(string directory)
    {
        var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var culture in new[] { "fa", "en", "ar", "es" })
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Find(Path.Combine(directory, culture + ".json"))));
            var texts = document.RootElement.GetProperty("texts");
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in texts.EnumerateObject())
            {
                values[property.Name] = property.Value.GetString() ?? string.Empty;
            }

            map[culture] = values;
        }

        return map;
    }

    private static string Find(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relativePath);
    }
}
