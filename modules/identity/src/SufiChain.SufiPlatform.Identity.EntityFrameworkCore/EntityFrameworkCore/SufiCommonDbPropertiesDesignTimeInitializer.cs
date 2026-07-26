using System.Runtime.CompilerServices;
using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

/// <summary>
/// Ensures <see cref="SufiCommonDbProperties"/> is applied to ABP common DB naming
/// when this assembly loads (including EF design-time DbContext discovery via
/// <see cref="ISufiIdentityDbContext"/>).
/// </summary>
internal static class SufiCommonDbPropertiesDesignTimeInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        SufiCommonDbProperties.ApplyToAbp();
    }
}
