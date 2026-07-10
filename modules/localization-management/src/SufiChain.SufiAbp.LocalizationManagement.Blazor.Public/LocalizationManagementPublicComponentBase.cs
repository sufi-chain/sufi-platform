using SufiChain.SufiAbp.LocalizationManagement;
using SufiChain.SufiAbp.LocalizationManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor.Public;

public abstract class LocalizationManagementPublicComponentBase : SufiAbpComponentBase
{
    protected LocalizationManagementPublicComponentBase()
    {
        LocalizationResource = typeof(SufiAbpLocalizationManagementResource);
    }

    protected IBusinessLocalizationEditorAppService BusinessLocalizationEditorAppService =>
        LazyGetRequiredService(ref _businessLocalizationEditorAppService);

    private IBusinessLocalizationEditorAppService? _businessLocalizationEditorAppService;
}
