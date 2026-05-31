namespace SufiChain.SufiAbp.FileManager.Features;

/// <summary>
/// Feature names for edition and plan gating of the File Manager module.
/// </summary>
public static class FileManagerFeatures
{
    public const string GroupName = "FileManager";

    public static class Names
    {
        /// <summary>
        /// Master switch for the File Manager module.
        /// </summary>
        public const string Enable = GroupName + ".Enable";

        /// <summary>
        /// Automatic file archiving background jobs.
        /// </summary>
        public const string Archiving = GroupName + ".Archiving";
    }
}
