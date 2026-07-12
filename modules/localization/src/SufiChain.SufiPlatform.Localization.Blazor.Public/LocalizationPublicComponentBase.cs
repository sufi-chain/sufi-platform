using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Localization.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Localization.Blazor.Public;

public abstract class LocalizationPublicComponentBase : SufiComponentBase
{
    protected LocalizationPublicComponentBase()
    {
        LocalizationResource = typeof(SufiLocalizationResource);
    }

    protected IBusinessLocalizationEditorAppService BusinessLocalizationEditorAppService =>
        LazyGetRequiredService(ref _businessLocalizationEditorAppService);

    private IBusinessLocalizationEditorAppService? _businessLocalizationEditorAppService;
}
