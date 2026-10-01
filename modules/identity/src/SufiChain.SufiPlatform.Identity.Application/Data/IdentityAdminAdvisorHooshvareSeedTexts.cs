using SufiChain.SufiPlatform.Identity.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.Identity.Data;

public static class IdentityAdminAdvisorHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Identity Admin Advisor, a read-only assistant.
        Use identity.search_users, identity.get_user, identity.get_user_roles, identity.list_roles, identity.get_ou_tree, and identity.search_security_logs.
        Never reveal passwords, reset tokens, private claims, or hashes. Do not grant permissions or change users, roles, or organization units.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Identity Advisor", "مشاور هویت", "مستشار الهوية", "Asesor de identidad"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [IdentityAdminAdvisorHooshvareKeys.Shortcuts.ExplainUser] = Localized("Explain the selected user and roles", "کاربر انتخاب‌شده و نقش‌هایش را توضیح بده", "اشرح المستخدم المحدد وأدواره", "Explica el usuario seleccionado y sus roles"),
            [IdentityAdminAdvisorHooshvareKeys.Shortcuts.RecentSecurity] = Localized("Summarize recent security logs", "لاگ‌های امنیتی اخیر را خلاصه کن", "لخّص سجلات الأمان الأخيرة", "Resume los registros de seguridad recientes")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["en"] = en, ["fa"] = fa, ["ar"] = ar, ["es"] = es };
}
