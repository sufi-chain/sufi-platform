namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

/// <summary>
/// Kind of platform hooshvare definition.
/// </summary>
public enum HooshvareKind
{
    /// <summary>
    /// Answers and reads. Its edit dialog offers read-only tools.
    /// </summary>
    Assistant = 0,

    /// <summary>
    /// Integrates into a module and can change that module's data.
    /// </summary>
    Copilot = 1,

    Agent = 2
}
