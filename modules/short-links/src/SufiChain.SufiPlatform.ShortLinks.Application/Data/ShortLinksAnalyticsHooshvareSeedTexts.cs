using SufiChain.SufiPlatform.ShortLinks.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.ShortLinks.Data;

public static class ShortLinksAnalyticsHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Short Link Analytics assistant. You only read links and click summaries.
        Use shortlinks.list, shortlinks.get, and shortlinks.get_analytics. Never invent click counts.
        Destination URLs omit query strings. Click results omit IP addresses. Do not create, update, or delete links.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Short Link Analytics", "تحلیل پیوند کوتاه", "تحليلات الروابط القصيرة", "Analítica de enlaces cortos"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [ShortLinksAnalyticsHooshvareKeys.Shortcuts.TopLinks] = Localized("Which short links have the most clicks?", "کدام پیوندهای کوتاه بیشترین کلیک را دارند؟", "أي الروابط القصيرة لديها أكبر عدد من النقرات؟", "¿Qué enlaces cortos tienen más clics?"),
            [ShortLinksAnalyticsHooshvareKeys.Shortcuts.ExplainSelection] = Localized("Explain analytics for the selected link", "تحلیل پیوند انتخاب‌شده را توضیح بده", "اشرح تحليلات الرابط المحدد", "Explica la analítica del enlace seleccionado")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
