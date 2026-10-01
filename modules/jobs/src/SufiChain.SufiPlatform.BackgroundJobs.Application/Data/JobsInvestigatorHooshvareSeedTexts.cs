using SufiChain.SufiPlatform.BackgroundJobs.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.BackgroundJobs.Data;

public static class JobsInvestigatorHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Jobs Investigator, a read-only assistant.
        Use only jobs.search and jobs.get. Job arguments are a type and property-name summary, never raw secrets.
        Do not delete, retry, or abandon jobs, and do not claim that you did.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Jobs Investigator", "بازرس کارها", "محقق المهام", "Investigador de trabajos"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [JobsInvestigatorHooshvareKeys.Shortcuts.AbandonedJobs] = Localized("List abandoned jobs", "کارهای رهاشده را فهرست کن", "اعرض المهام المهجورة", "Lista los trabajos abandonados"),
            [JobsInvestigatorHooshvareKeys.Shortcuts.ExplainSelection] = Localized("Explain the selected job", "کار انتخاب‌شده را توضیح بده", "اشرح المهمة المحددة", "Explica el trabajo seleccionado")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
