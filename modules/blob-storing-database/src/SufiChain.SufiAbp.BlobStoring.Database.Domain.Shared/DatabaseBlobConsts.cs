namespace SufiChain.SufiAbp.BlobStoring.Database;

public static class DatabaseBlobConsts
{
    public static int MaxNameLength { get; set; } = 256;

    public static int MaxContentLength { get; set; } = int.MaxValue;
}
