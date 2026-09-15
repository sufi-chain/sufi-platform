namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Resolved chat-composer toolbar flags for a copilot host.
/// Persist defaults on <see cref="CopilotRuntimeOptions"/> (plus
/// <c>AllowUserModelSelection</c> on the definition). Hosts such as LiveChat
/// inboxes may override individual flags; <see langword="null"/> means inherit.
/// </summary>
public class CopilotComposerToolbarOptions
{
    public bool AllowEmoji { get; set; } = true;

    public bool AllowAudio { get; set; }

    public bool AllowAttachment { get; set; }

    public bool AllowModelSelector { get; set; }

    public bool AllowContext { get; set; }

    public static CopilotComposerToolbarOptions Default { get; } = new();

    public static CopilotComposerToolbarOptions FromRuntime(
        CopilotRuntimeOptions? runtimeOptions,
        bool allowUserModelSelection)
    {
        runtimeOptions ??= new CopilotRuntimeOptions();
        return new CopilotComposerToolbarOptions
        {
            AllowEmoji = runtimeOptions.AllowComposerEmoji,
            AllowAudio = runtimeOptions.AllowComposerAudio,
            AllowAttachment = runtimeOptions.AllowComposerAttachment,
            AllowModelSelector = allowUserModelSelection,
            AllowContext = runtimeOptions.AllowComposerContext
        };
    }

    public static CopilotComposerToolbarOptions Resolve(
        CopilotComposerToolbarOptions? inherited,
        bool? allowEmoji,
        bool? allowAudio,
        bool? allowAttachment,
        bool? allowModelSelector,
        bool? allowContext)
    {
        var source = inherited ?? Default;
        return new CopilotComposerToolbarOptions
        {
            AllowEmoji = allowEmoji ?? source.AllowEmoji,
            AllowAudio = allowAudio ?? source.AllowAudio,
            AllowAttachment = allowAttachment ?? source.AllowAttachment,
            AllowModelSelector = allowModelSelector ?? source.AllowModelSelector,
            AllowContext = allowContext ?? source.AllowContext
        };
    }
}
