using SufiChain.SufiPlatform.AuditLogging.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.AuditLogging.Blazor;

/// <summary>
/// Base class for Blazor components in the Audit Logging module.
/// Provides module localization via AuditLoggingResource.
/// </summary>
public abstract class AuditLoggingComponentBase : SufiComponentBase
{
    protected AuditLoggingComponentBase()
    {
        LocalizationResource = typeof(SufiAuditLoggingResource);
    }
}
