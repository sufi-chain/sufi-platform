using Volo.Abp.AspNetCore.Mvc;

namespace SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

/// <summary>
/// Base controller for SufiAbp HTTP API controllers.
/// Provides common functionality and extension points for all SufiAbp controllers.
/// </summary>
public abstract class SufiAbpControllerBase : AbpControllerBase
{
    // Extension point for future platform-wide HTTP concerns:
    // - Custom error handling
    // - Response wrapping
    // - Telemetry/metrics
    // - Security headers
}
