namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Decides whether the anonymous public menu API may read one context.
/// A module returns null when it does not own that context type.
/// </summary>
public interface IPublicMenuContextVisibility
{
    /// <summary>
    /// True allows the read, false denies it, and null leaves the decision to another policy.
    /// A false result denies the read even when another policy allows the same context.
    /// </summary>
    Task<bool?> IsPubliclyVisibleAsync(string contextType, Guid? contextId);
}
