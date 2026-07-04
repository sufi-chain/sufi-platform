using SufiChain.SufiAbp.TagsManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.TagsManagement.Blazor;

/// <summary>
/// Base class for Blazor components in the Tags Management module.
/// Provides module localization via SufiAbpTagsManagementResource.
/// </summary>
public abstract class TagsManagementComponentBase : SufiAbpComponentBase
{
    protected TagsManagementComponentBase()
    {
        LocalizationResource = typeof(SufiAbpTagsManagementResource);
    }
}
