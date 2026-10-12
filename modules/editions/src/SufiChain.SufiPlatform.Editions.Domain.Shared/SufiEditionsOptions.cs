using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Localization resources tried when resolving an edition's stored display name key.
/// Other modules (for example SufiSaas) append their resource names so plan keys resolve
/// without the Editions module referencing those modules.
/// </summary>
public class SufiEditionsOptions
{
    /// <summary>
    /// Resource names passed to <c>BusinessLocalizationHelper.ResolveText</c>, in order.
    /// Defaults to the Editions resource.
    /// </summary>
    public List<string> DisplayNameResources { get; } = ["Editions"];
}
