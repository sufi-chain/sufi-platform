namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

/// <summary>
/// Immutable platform hooshvare seed payload supplied by each owning module's data seed contributor.
/// </summary>
public class PlatformHooshvareSeedDefinition
{
    public string Key { get; set; } = string.Empty;

    public string SourceModule { get; set; } = string.Empty;

    public string? RequiredFeatureName { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public HooshvareKind Kind { get; set; }

    /// <summary>
    /// Localization resource name for business-tier text (matches module localization resource).
    /// When empty, falls back to the hooshvares default resource name.
    /// </summary>
    public string? LocalizationResourceName { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public bool PersistChatSession { get; set; }

    public bool IsPublic { get; set; }

    /// <summary>
    /// Required for public hooshvares. Confirms that the owning module prompt and entry point
    /// define uncertainty, privacy, unsupported-claim, and human-escalation behavior.
    /// </summary>
    public bool HasPublicSafetyPolicy { get; set; }

    /// <summary>
    /// Applied only when the definition is first inserted. Later seed versions preserve
    /// the administrator's current enabled or disabled state.
    /// </summary>
    public bool DefaultEnabled { get; set; } = true;

    public string SystemPrompt { get; set; } = string.Empty;

    public HooshvareRuntimeOptions RuntimeOptions { get; set; } = new();

    public string? ShortcutPromptsJson { get; set; }

    public Dictionary<string, HooshvareShortcutCapability> ShortcutCapabilities { get; set; } = new();

    public List<string> RequiredContextKeys { get; set; } = new();

    /// <summary>
    /// When true, end users may pick a chat route inside the bound workspace.
    /// This is a seed capability, not a tenant override.
    /// </summary>
    public bool AllowUserModelSelection { get; set; }

    /// <summary>
    /// Optional route allowlist. Empty means every enabled, user-selectable chat route
    /// in the bound workspace. A non-empty list narrows that set.
    /// </summary>
    public List<Guid> AllowedModelConfigurationIds { get; set; } = new();

    /// <summary>
    /// Seed generation. Existing rows update only when this value is greater than the stored entity version.
    /// Fresh beta seeds start at 0. New definitions are created at -1 so the first seed apply succeeds.
    /// </summary>
    public int EntityVersion { get; set; } = 0;
}
