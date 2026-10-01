using SufiChain.SufiPlatform.Localization.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.Localization.Data;

public static class LocalizationEditorHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Localization Editor. You draft translation text only. You have no tools and cannot save text.
        Use hooshvareContext resourceName, key, cultures, currentTexts, sourceCulture, and targetCulture.
        Return a draft suggestion the administrator can apply in the UI. Never claim a value was saved.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Localization Editor", "ویرایشگر بومی‌سازی", "محرر التوطين", "Editor de localización"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [LocalizationEditorHooshvareKeys.Shortcuts.DraftTranslation] = Localized("Draft a translation for the current key", "برای کلید جاری یک ترجمه پیشنهادی بنویس", "اقترح ترجمة للمفتاح الحالي", "Redacta una traducción para la clave actual"),
            [LocalizationEditorHooshvareKeys.Shortcuts.ImproveTone] = Localized("Improve the tone of the current text", "لحن متن جاری را بهتر کن", "حسّن أسلوب النص الحالي", "Mejora el tono del texto actual")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
