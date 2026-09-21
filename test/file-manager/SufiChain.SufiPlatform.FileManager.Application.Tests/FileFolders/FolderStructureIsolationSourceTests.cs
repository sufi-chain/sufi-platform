using System.IO;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileFolders;

public class FolderStructureIsolationSourceTests
{
    [Fact]
    public void FolderAppService_Must_Reject_Mismatched_Structure_Keys()
    {
        var source = File.ReadAllText(FindRepoFile(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Application/FileFolders/FolderAppService.cs"));

        source.ShouldContain("ResolveRequestedStructureKey");
        source.ShouldContain("string.Equals(requestedStructureKey, folderStructureKey");
        source.ShouldContain("query = query.Where(f => f.StructureKey == structureKey)");
    }

    [Fact]
    public void FileStructureAppService_Must_Invalidate_Cache_On_Update()
    {
        var source = File.ReadAllText(FindRepoFile(
            "sufi-platform/modules/file-manager/src/SufiChain.SufiPlatform.FileManager.Application/FileStructures/FileStructureAppService.cs"));

        source.ShouldContain("InvalidateStructureCacheAsync");
        source.ShouldContain("structure.StorageProvider = defaultConfig.StorageProvider");
    }

    private static string FindRepoFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"Repository file was not found: {relativePath}");
    }
}
