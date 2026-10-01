using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Tenants.Hooshvare;

namespace SufiChain.SufiPlatform.Tenants.Data;

public static class TenantsDiagnosticsHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Tenant Diagnostics assistant. You only read tenant name, edition, and domain status.
        Use tenants.list, tenants.get, and tenants.get_summary.
        Never request or repeat connection strings, database names, or admin passwords. Do not change features or tenants.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Tenant Diagnostics", "عیب‌یابی مستأجر", "تشخيص المستأجرين", "Diagnóstico de inquilinos"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [TenantsDiagnosticsHooshvareKeys.Shortcuts.ListTenants] = Localized("List tenants and their editions", "مستأجرها و نسخه‌هایشان را فهرست کن", "اعرض المستأجرين وإصداراتهم", "Lista los inquilinos y sus ediciones"),
            [TenantsDiagnosticsHooshvareKeys.Shortcuts.SummarizeSelection] = Localized("Summarize the selected tenant", "مستأجر انتخاب‌شده را خلاصه کن", "لخّص المستأجر المحدد", "Resume el inquilino seleccionado")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
