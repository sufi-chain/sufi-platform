using System;
using System.Collections.Generic;
using System.Text;

namespace SufiChain.SufiAbp.Data;
public static class SufiAbpCommonDbProperties
{
    /// <summary>
    /// This table prefix is shared by most of the ABP modules.
    /// You can change it to set table prefix for all modules using this.
    /// 
    /// Default value: "Abp".
    /// </summary>
    public static string DbTablePrefix { get; set; } = null;

    /// <summary>
    /// Default value: null.
    /// </summary>
    public static string? DbSchema { get; set; } = null;
}
