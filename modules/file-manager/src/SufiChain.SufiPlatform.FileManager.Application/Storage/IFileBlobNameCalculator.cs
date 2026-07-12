using System;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public interface IFileBlobNameCalculator
{
    /// <summary>
    /// Calculate blob name for a file item
    /// </summary>
    string Calculate(
        Guid fileId,
        string fileName,
        bool isTemp,
        string? structureKey = null);
}
