using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.FileManager;

public abstract class FileManagerAppService : SufiApplicationService
{
    protected FileManagerAppService()
    {
        LocalizationResource = typeof(SufiFileManagerResource);
        ObjectMapperContext = typeof(SufiFileManagerApplicationModule);
    }
}