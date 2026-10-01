namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Where a chat route's image, file, and reasoning flags came from.
/// </summary>
public enum ModelCapabilitySource
{
    LiveCatalog = 0,
    ExternalCatalog = 1,
    Manual = 2
}
