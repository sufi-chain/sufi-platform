using SufiChain.SufiAbp.AuditLogging.Localization;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.AuditLogging.Blazor;

/// <summary>
/// Base class for Blazor components in the Audit Logging module.
/// Provides module localization via AuditLoggingResource.
/// </summary>
public abstract class AuditLoggingComponentBase : SufiAbpComponentBase
{
    protected AuditLoggingComponentBase()
    {
        LocalizationResource = typeof(SufiAbpAuditLoggingResource);
    }
}
