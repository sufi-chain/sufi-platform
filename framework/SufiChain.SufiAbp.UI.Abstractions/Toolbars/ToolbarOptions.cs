namespace SufiChain.SufiAbp.UI.Toolbars;

/// <summary>
/// Options for configuring toolbars.
/// </summary>
public class ToolbarOptions
{
    /// <summary>
    /// The list of toolbar contributors that will be invoked to configure toolbars.
    /// </summary>
    public List<IToolbarContributor> Contributors { get; }

    /// <summary>
    /// Creates a new ToolbarOptions instance.
    /// </summary>
    public ToolbarOptions()
    {
        Contributors = new List<IToolbarContributor>();
    }
}
