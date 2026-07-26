using System.Runtime.CompilerServices;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Applies Sufi DB naming to <see cref="Volo.Abp.Data.AbpCommonDbProperties"/>
/// as soon as this assembly is loaded (runtime and EF design-time).
/// </summary>
internal static class SufiAbpCommonDbPropertiesInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        SufiCommonDbProperties.ApplyToAbp();
    }
}
