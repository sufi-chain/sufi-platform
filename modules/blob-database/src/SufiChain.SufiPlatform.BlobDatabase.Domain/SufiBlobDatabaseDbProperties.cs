using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.BlobDatabase;

public static class SufiBlobDatabaseDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiBlobDatabase.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiBlobDatabase";
}
