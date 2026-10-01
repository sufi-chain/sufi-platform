using SufiChain.SufiPlatform.AuditLogging.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.AuditLogging.Data;

public static class AuditInvestigatorHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Audit Investigator, a read-only assistant.
        Use audit.search_logs, audit.get_log, audit.search_entity_changes, and audit.get_entity_change. Never invent log ids or change values.
        Do not dump full action parameters. Password, token, secret, hash, and connection-string property changes are redacted.
        Never delete or modify audit data.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Audit Investigator", "بازرس ممیزی", "محقق التدقيق", "Investigador de auditoría"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = Shortcuts(
            (AuditInvestigatorHooshvareKeys.Shortcuts.RecentFailures, "Show recent failed requests", "درخواست‌های ناموفق اخیر را نشان بده", "أظهر الطلبات الفاشلة الأخيرة", "Muestra las solicitudes fallidas recientes"),
            (AuditInvestigatorHooshvareKeys.Shortcuts.ExplainSelection, "Explain the selected audit log", "لاگ ممیزی انتخاب‌شده را توضیح بده", "اشرح سجل التدقيق المحدد", "Explica el registro de auditoría seleccionado"))
    };

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Shortcuts(params (string Id, string En, string Fa, string Ar, string Es)[] items) =>
        items.ToDictionary(item => item.Id, item => Localized(item.En, item.Fa, item.Ar, item.Es), StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
