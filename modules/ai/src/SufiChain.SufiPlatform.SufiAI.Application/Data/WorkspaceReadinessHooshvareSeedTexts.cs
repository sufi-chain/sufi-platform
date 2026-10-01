using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public static class WorkspaceReadinessHooshvareSeedTexts
{
    private const string SystemPrompt = """
        You are the Workspace Readiness assistant. You only read AI workspace status.
        Call ai.list_workspaces, ai.get_workspace_readiness, ai.list_model_capabilities, ai.get_rag_availability, and ai.get_mcp_catalog_summary. Never invent readiness, model ids, or tool names.
        Never reveal API keys, endpoint secrets, MCP argument JSON, or raw provider payloads. HasApiKey is a boolean, not a secret.
        Do not create, update, delete, or test connections.
        """;

    public static HooshvareSeedTexts Texts { get; } = new()
    {
        DisplayName = Localized("Workspace Readiness", "آمادگی فضای کاری", "جاهزية مساحة العمل", "Preparación del espacio"),
        SystemPrompt = Localized(SystemPrompt, SystemPrompt, SystemPrompt, SystemPrompt),
        Shortcuts = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            [WorkspaceReadinessHooshvareKeys.Shortcuts.SummarizeReadiness] = Localized(
                "Summarize readiness for the selected workspace",
                "آمادگی فضای کاری انتخاب‌شده را خلاصه کن",
                "لخّص جاهزية مساحة العمل المحددة",
                "Resume la preparación del espacio seleccionado"),
            [WorkspaceReadinessHooshvareKeys.Shortcuts.CheckRag] = Localized(
                "Check whether RAG is available",
                "بررسی کن RAG در دسترس است یا نه",
                "تحقق مما إذا كان RAG متاحًا",
                "Comprueba si RAG está disponible")
        }
    };

    private static IReadOnlyDictionary<string, string> Localized(string en, string fa, string ar, string es) =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = en,
            ["fa"] = fa,
            ["ar"] = ar,
            ["es"] = es
        };
}
