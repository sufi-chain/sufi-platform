using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.FileManager;

public abstract class FileManagerAppService : SufiAbpApplicationService
{
    protected FileManagerAppService()
    {
        LocalizationResource = typeof(SufiAbpFileManagerResource);
        ObjectMapperContext = typeof(SufiAbpFileManagerApplicationModule);
    }
}
