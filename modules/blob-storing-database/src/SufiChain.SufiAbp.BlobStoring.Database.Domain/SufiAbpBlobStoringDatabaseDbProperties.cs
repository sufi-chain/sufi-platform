using Volo.Abp.Data;

namespace SufiChain.SufiAbp.BlobStoring.Database;

public static class SufiAbpBlobStoringDatabaseDbProperties
{
    public static string DbTablePrefix { get; set; } = "BlobStoring.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "BlobStoring";
}
