using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.FileManager;

public abstract class FileManagerController : SufiAbpControllerBase
{
    protected FileManagerController()
    {
        LocalizationResource = typeof(SufiAbpFileManagerResource);
    }
}
