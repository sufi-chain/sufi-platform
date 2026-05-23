using Volo.Abp.Data;

namespace SufiChain.SufiAbp.SettingManagement;

public static class SufiAbpSettingManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Settings.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpSettingManagement";
}
