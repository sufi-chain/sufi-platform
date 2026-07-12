using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.FileManager;

public abstract class FileManagerController : SufiControllerBase
{
    protected FileManagerController()
    {
        LocalizationResource = typeof(SufiFileManagerResource);
    }
}