namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Shared data-seeding defaults for host and tenant seed passes.
/// </summary>
public class SufiDataSeedOptions
{
    /// <summary>
    /// Default culture used when seeding business localization text and content entities.
    /// </summary>
    public string DefaultCulture { get; set; } = "fa";

    /// <summary>
    /// Cultures seeded for every business localization key.
    /// </summary>
    public string[] SupportedCultures { get; set; } = new[] { "fa", "en", "ar", "es" };
}
