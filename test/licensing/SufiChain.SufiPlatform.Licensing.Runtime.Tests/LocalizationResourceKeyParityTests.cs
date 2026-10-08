using System.Text.Json;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Licensing;

public class LocalizationResourceKeyParityTests
{
    public static TheoryData<string> EqualFaAndEnResources { get; } = new()
    {
        "sufi-platform/modules/menus/src/SufiChain.SufiPlatform.Menus.Domain.Shared/Localization/Menus",
        "sufi-platform/modules/tags/src/SufiChain.SufiPlatform.Tags.Domain.Shared/Localization/Tags",
        "sufi-platform/modules/localization/src/SufiChain.SufiPlatform.Localization.Domain.Shared/Localization/Localization",
        "sufi-platform/framework/SufiChain.SufiPlatform.SufiCom/Localization/SufiComFramework",
        "commercial-modules/sufi-licensing/src/SufiChain.SufiPlatform.Licensing.Abstractions/Localization/Licensing",
        "commercial-modules/sufi-saas/src/SufiChain.SufiPlatform.SufiSaas.Domain.Shared/Localization/SufiSaas",
        "pro-modules/dashboard/src/SufiChain.SufiPlatform.Dashboard.Domain.Shared/Localization/Dashboard",
        "pro-modules/crm/src/SufiChain.SufiPlatform.SufiCRM.Contacts.Domain.Shared/Localization/Contacts",
        "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase",
        "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Domain.Shared/Localization/Ticketing",
        "pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Domain.Shared/Localization/SufiAIHooshvare"
    };

    [Theory]
    [MemberData(nameof(EqualFaAndEnResources))]
    public void Fa_and_en_resource_files_have_the_same_keys(string relativeDirectory)
    {
        var directory = Path.Combine(FindWorkspaceRoot(), relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
        var english = ReadKeys(Path.Combine(directory, "en.json"));
        var persian = ReadKeys(Path.Combine(directory, "fa.json"));

        persian.Except(english).ShouldBeEmpty();
        english.Except(persian).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("sufi-platform/modules/tags/src/SufiChain.SufiPlatform.Tags.Domain.Shared/Localization/Tags", "DeleteTag", "Delete tag", "حذف برچسب", "حذف الوسم", "Eliminar etiqueta")]
    [InlineData("sufi-platform/modules/identity/src/SufiChain.SufiPlatform.Identity.Domain.Shared/Localization/Identity", "Static", "Built-in", "پیش\u200cساخته", "مدمج", "Integrado")]
    [InlineData("sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Domain.Shared/Localization/FileManager", "Statistics", "Statistics", "آمار", "الإحصاءات", "Estadísticas")]
    [InlineData("sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Domain.Shared/Localization/FileManager", "Images", "Images", "تصاویر", "الصور", "Imágenes")]
    [InlineData("sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Domain.Shared/Localization/FileManager", "Videos", "Videos", "ویدیوها", "مقاطع الفيديو", "Vídeos")]
    [InlineData("sufi-platform/modules/menus/src/SufiChain.SufiPlatform.Menus.Domain.Shared/Localization/Menus", "DeleteMenu", "Delete menu", "حذف منو", "حذف القائمة", "Eliminar menú")]
    [InlineData("sufi-platform/modules/localization/src/SufiChain.SufiPlatform.Localization.Domain.Shared/Localization/Localization", "SupportedCultures", "Supported languages", "زبان\u200cهای پشتیبانی\u200cشده", "اللغات المدعومة", "Idiomas admitidos")]
    [InlineData("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase", "Version", "Version", "نسخه", "الإصدار", "Versión")]
    [InlineData("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase", "KnowledgeBase:Select", "Select", "انتخاب", "تحديد", "Seleccionar")]
    [InlineData("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase", "Breadcrumb", "Breadcrumb", "مسیر صفحه", "مسار التنقل", "Ruta de navegación")]
    [InlineData("pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase", "Updated", "Last updated", "آخرین به\u200cروزرسانی", "آخر تحديث", "Última actualización")]
    public void Raw_keys_exist_in_english_persian_arabic_and_spanish(
        string relativeDirectory,
        string key,
        string english,
        string persian,
        string arabic,
        string spanish)
    {
        var directory = Path.Combine(FindWorkspaceRoot(), relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
        ReadTexts(Path.Combine(directory, "en.json"))[key].ShouldBe(english);
        ReadTexts(Path.Combine(directory, "fa.json"))[key].ShouldBe(persian);
        ReadTexts(Path.Combine(directory, "ar.json"))[key].ShouldBe(arabic);
        ReadTexts(Path.Combine(directory, "es.json"))[key].ShouldBe(spanish);
    }

    [Fact]
    public void My_workspaces_status_uses_the_existing_tenant_request_status_keys()
    {
        var markup = File.ReadAllText(Path.Combine(
            FindWorkspaceRoot(),
            "commercial-modules",
            "sufi-saas",
            "src",
            "SufiChain.SufiPlatform.SufiSaas.Blazor",
            "Pages",
            "MyWorkspaces.razor"));

        markup.ShouldContain("L[$\"TenantRequestStatus:{ctx.Item.Status}\"]");
        markup.ShouldNotContain(">@ctx.Item.Status</SbChip>");
    }

    [Fact]
    public void Touched_persian_files_do_not_keep_the_rejected_help_desk_or_hooshvare_terms()
    {
        var root = FindWorkspaceRoot();
        var contacts = File.ReadAllText(Path.Combine(root, "pro-modules/crm/src/SufiChain.SufiPlatform.SufiCRM.Contacts.Domain.Shared/Localization/Contacts/fa.json".Replace('/', Path.DirectorySeparatorChar)));
        var hooshvare = File.ReadAllText(Path.Combine(root, "pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Domain.Shared/Localization/SufiAIHooshvare/fa.json".Replace('/', Path.DirectorySeparatorChar)));
        var knowledgeBase = File.ReadAllText(Path.Combine(root, "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared/Localization/KnowledgeBase/fa.json".Replace('/', Path.DirectorySeparatorChar)));
        var ticketing = File.ReadAllText(Path.Combine(root, "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Domain.Shared/Localization/Ticketing/fa.json".Replace('/', Path.DirectorySeparatorChar)));
        var saas = File.ReadAllText(Path.Combine(root, "commercial-modules/sufi-saas/src/SufiChain.SufiPlatform.SufiSaas.Domain.Shared/Localization/SufiSaas/fa.json".Replace('/', Path.DirectorySeparatorChar)));
        var saasEnglish = File.ReadAllText(Path.Combine(root, "commercial-modules/sufi-saas/src/SufiChain.SufiPlatform.SufiSaas.Domain.Shared/Localization/SufiSaas/en.json".Replace('/', Path.DirectorySeparatorChar)));
        var roles = File.ReadAllText(Path.Combine(root, "sufi-platform/modules/identity/src/SufiChain.SufiPlatform.Identity.Blazor/Pages/RoleManagement.razor".Replace('/', Path.DirectorySeparatorChar)));

        contacts.ShouldNotContain("میز خدمت");
        hooshvare.ShouldNotContain("میز خدمت");
        knowledgeBase.ShouldNotContain("میز خدمت");
        ticketing.ShouldNotContain("میز کمک");
        saas.ShouldNotContain("Community Education");
        saasEnglish.ShouldNotContain("Community Education");
        saasEnglish.ShouldContain("Community Edition");
        roles.ShouldContain("L[\"PublicRole\"]");
        Regex.IsMatch(hooshvare, "هوشوار(?!ه)").ShouldBeFalse();
        knowledgeBase.ShouldContain("راهنمای سکوی صوفی");
        saas.ShouldContain("نسخه\u0654 جامعه");
    }

    private static HashSet<string> ReadKeys(string path)
    {
        return ReadTexts(path).Keys.ToHashSet(StringComparer.Ordinal);
    }

    private static Dictionary<string, string> ReadTexts(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement texts = default;
        var found = false;
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name.Equals("texts", StringComparison.OrdinalIgnoreCase))
            {
                texts = property.Value;
                found = true;
                break;
            }
        }

        found.ShouldBeTrue();
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in texts.EnumerateObject())
        {
            values[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return values;
    }

    private static string FindWorkspaceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var licensing = Path.Combine(
                dir.FullName,
                "commercial-modules",
                "sufi-licensing",
                "src",
                "SufiChain.SufiPlatform.Licensing.Abstractions",
                "Localization",
                "Licensing",
                "en.json");
            var menus = Path.Combine(
                dir.FullName,
                "sufi-platform",
                "modules",
                "menus",
                "src",
                "SufiChain.SufiPlatform.Menus.Domain.Shared",
                "Localization",
                "Menus",
                "en.json");
            if (File.Exists(licensing) && File.Exists(menus))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("Workspace root with sufi-platform and commercial licensing was not found.");
    }
}
