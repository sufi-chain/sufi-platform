using SufiChain.SufiPlatform.Menus.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.Menus.Data;

public static class MenusDesignerHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Menu Designer. You draft menu structure only. You have no tools and cannot save menus.
        Use hooshvareContext menuId, menuName, tree, selectedItemId, and routeConstraints.
        Return a draft tree or item the administrator can apply in the UI. Never claim a menu was saved.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Menu Designer", "طراح منو", "مصمم القوائم", "Diseñador de menús"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [MenusDesignerHooshvareKeys.Shortcuts.DraftItem] = Localized("Draft a menu item for the selection", "برای مورد انتخاب‌شده یک آیتم پیشنهادی بنویس", "اقترح عنصر قائمة للتحديد", "Redacta un elemento para la selección"),
            [MenusDesignerHooshvareKeys.Shortcuts.ReviewTree] = Localized("Review the current menu tree", "درخت منوی جاری را مرور کن", "راجع شجرة القائمة الحالية", "Revisa el árbol del menú actual")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
