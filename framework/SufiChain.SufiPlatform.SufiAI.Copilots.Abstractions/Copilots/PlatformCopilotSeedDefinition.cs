namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Immutable platform copilot seed payload supplied by each owning module's data seed contributor.
/// </summary>
public class PlatformCopilotSeedDefinition
{
    public string Key { get; set; } = string.Empty;

    public string SourceModule { get; set; } = string.Empty;

    public string? RequiredFeatureName { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public CopilotKind Kind { get; set; }

    /// <summary>
    /// Localization resource name for business-tier text (matches module localization resource).
    /// When empty, falls back to the copilots default resource name.
    /// </summary>
    public string? LocalizationResourceName { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public bool PersistChatSession { get; set; }

    public bool IsPublic { get; set; }

    /// <summary>
    /// Required for public copilots. Confirms that the owning module prompt and entry point
    /// define uncertainty, privacy, unsupported-claim, and human-escalation behavior.
    /// </summary>
    public bool HasPublicSafetyPolicy { get; set; }

    /// <summary>
    /// Applied only when the definition is first inserted. Later seed versions preserve
    /// the administrator's current enabled or disabled state.
    /// </summary>
    public bool DefaultEnabled { get; set; } = true;

    public string SystemPrompt { get; set; } = string.Empty;

    public CopilotRuntimeOptions RuntimeOptions { get; set; } = new();

    public string? ShortcutPromptsJson { get; set; }

    public Dictionary<string, CopilotShortcutCapability> ShortcutCapabilities { get; set; } = new();

    public List<string> RequiredContextKeys { get; set; } = new();

    /// <summary>
    /// Bump when seed content changes. Existing rows update only when greater than stored entity version.
    /// </summary>
    public int EntityVersion { get; set; } = 1;
}
