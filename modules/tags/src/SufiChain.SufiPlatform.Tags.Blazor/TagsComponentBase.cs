using SufiChain.SufiPlatform.Tags.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Tags.Blazor;

/// <summary>
/// Base class for Blazor components in the Tags Management module.
/// Provides module localization via SufiTagsResource.
/// </summary>
public abstract class TagsComponentBase : SufiComponentBase
{
    protected TagsComponentBase()
    {
        LocalizationResource = typeof(SufiTagsResource);
    }
}