using SufiChain.SufiAbp.UI.Toolbars;

namespace SufiChain.Chat.Blazor.Toolbars;

/// <summary>
/// Contributes the chat inbox icon to the host toolbar so it appears next to
/// language and theme switches in the KomTheme icon rail footer.
/// </summary>
/// <remarks>
/// The constant <c>KomMain</c> matches <c>SufiChain.KomTheme.KomToolbars.Main</c>
/// without taking a direct dependency on the KomTheme product package.
/// </remarks>
public class ChatToolbarContributor : IToolbarContributor
{
    /// <summary>
    /// Default name for the KomTheme main toolbar that hosts language/theme switches.
    /// </summary>
    public const string DefaultToolbarName = "KomMain";

    /// <summary>
    /// Order placed between the theme switch (100) and language switch (200)
    /// so the chat inbox icon sits in the same lower-rail toolbar cluster.
    /// </summary>
    public const int DefaultOrder = 150;

    public Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name != DefaultToolbarName)
        {
            return Task.CompletedTask;
        }

        context.Toolbar.Items.Add(new ToolbarItem(typeof(ChatInboxToolbarComponent), DefaultOrder));
        return Task.CompletedTask;
    }
}
