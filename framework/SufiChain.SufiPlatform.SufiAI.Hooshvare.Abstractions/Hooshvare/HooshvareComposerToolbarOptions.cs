namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



/// <summary>

/// Resolved chat-composer toolbar flags for a hooshvare host.

/// Persist defaults on <see cref="HooshvareRuntimeOptions"/> (plus

/// <c>AllowUserModelSelection</c> on the definition). Hosts such as LiveChat

/// inboxes may override individual flags; <see langword="null"/> means inherit.

/// </summary>

public class HooshvareComposerToolbarOptions

{

    public bool AllowEmoji { get; set; } = true;



    public bool AllowAudio { get; set; }



    public bool AllowAttachment { get; set; }



    public bool AllowModelSelector { get; set; }



    public bool AllowContext { get; set; }



    public static HooshvareComposerToolbarOptions Default { get; } = new();



    public static HooshvareComposerToolbarOptions FromRuntime(

        HooshvareRuntimeOptions? runtimeOptions,

        bool allowUserModelSelection)

    {

        runtimeOptions ??= new HooshvareRuntimeOptions();

        return new HooshvareComposerToolbarOptions

        {

            AllowEmoji = runtimeOptions.AllowComposerEmoji,

            AllowAudio = runtimeOptions.AllowComposerAudio,

            AllowAttachment = runtimeOptions.AllowComposerAttachment,

            AllowModelSelector = allowUserModelSelection,

            AllowContext = runtimeOptions.AllowComposerContext

        };

    }



    public static HooshvareComposerToolbarOptions Resolve(

        HooshvareComposerToolbarOptions? inherited,

        bool? allowEmoji,

        bool? allowAudio,

        bool? allowAttachment,

        bool? allowModelSelector,

        bool? allowContext)

    {

        var source = inherited ?? Default;

        return new HooshvareComposerToolbarOptions

        {

            AllowEmoji = allowEmoji ?? source.AllowEmoji,

            AllowAudio = allowAudio ?? source.AllowAudio,

            AllowAttachment = allowAttachment ?? source.AllowAttachment,

            AllowModelSelector = allowModelSelector ?? source.AllowModelSelector,

            AllowContext = allowContext ?? source.AllowContext

        };

    }

}

