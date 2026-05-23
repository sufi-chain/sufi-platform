using SufiChain.SufiAbp.FileManager.Localization;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.FileManager;

public abstract class FileManagerAppService : ApplicationService
{
    protected FileManagerAppService()
    {
        LocalizationResource = typeof(SufiAbpFileManagerResource);
        ObjectMapperContext = typeof(SufiAbpFileManagerApplicationModule);
    }
}
