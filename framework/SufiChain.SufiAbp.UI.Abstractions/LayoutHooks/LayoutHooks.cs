namespace SufiChain.SufiAbp.UI.LayoutHooks;

/// <summary>
/// Standard layout hook names for injecting components into layouts.
/// </summary>
public static class LayoutHooks
{
    /// <summary>
    /// Hooks for the HTML head section.
    /// </summary>
    public static class Head
    {
        /// <summary>
        /// First position in the head.
        /// </summary>
        public const string First = "Header.First";

        /// <summary>
        /// Last position in the head.
        /// </summary>
        public const string Last = "Header.Last";
    }

    /// <summary>
    /// Hooks for the body section.
    /// </summary>
    public static class Body
    {
        /// <summary>
        /// First position in the body.
        /// </summary>
        public const string First = "Body.First";

        /// <summary>
        /// Last position in the body.
        /// </summary>
        public const string Last = "Body.Last";
    }

    /// <summary>
    /// Hooks for the page content area.
    /// </summary>
    public static class PageContent
    {
        /// <summary>
        /// First position in the page content.
        /// </summary>
        public const string First = "PageContent.First";

        /// <summary>
        /// Last position in the page content.
        /// </summary>
        public const string Last = "PageContent.Last";
    }
}
