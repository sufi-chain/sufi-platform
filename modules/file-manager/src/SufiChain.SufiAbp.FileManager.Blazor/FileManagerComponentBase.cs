using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.FileManager.Blazor;

/// <summary>
/// Base class for Blazor components in the File Manager module.
/// Provides module localization via FileManagerResource.
/// </summary>
public abstract class FileManagerComponentBase : SufiAbpComponentBase
{
    protected FileManagerComponentBase()
    {
        LocalizationResource = typeof(SufiAbpFileManagerResource);
    }
}
