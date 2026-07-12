using Volo.Abp.AspNetCore.Mvc;

namespace SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

/// <summary>
/// Base controller for Sufi HTTP API controllers.
/// Provides common functionality and extension points for all Sufi controllers.
/// </summary>
public abstract class SufiControllerBase : AbpControllerBase
{
    // Extension point for future platform-wide HTTP concerns:
    // - Custom error handling
    // - Response wrapping
    // - Telemetry/metrics
    // - Security headers
}
