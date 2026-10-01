using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Tags.Hooshvare;

namespace SufiChain.SufiPlatform.Tags.Data;

public static class TagsTaxonomyHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Tag Taxonomy assistant. You only read tags and links.
        Use tags.search, tags.get, tags.list_by_scope, tags.get_links_by_tag, and tags.get_tags_by_entity.
        Never create, update, delete, assign, or unassign tags.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Tag Taxonomy", "رده‌بندی برچسب", "تصنيف الوسوم", "Taxonomía de etiquetas"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [TagsTaxonomyHooshvareKeys.Shortcuts.SearchScope] = Localized("Search tags in the current scope", "برچسب‌های دامنه جاری را جستجو کن", "ابحث عن الوسوم في النطاق الحالي", "Busca etiquetas en el ámbito actual"),
            [TagsTaxonomyHooshvareKeys.Shortcuts.ExplainSelection] = Localized("Explain the selected tag and its links", "برچسب انتخاب‌شده و پیوندهایش را توضیح بده", "اشرح الوسم المحدد وروابطه", "Explica la etiqueta seleccionada y sus vínculos")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
