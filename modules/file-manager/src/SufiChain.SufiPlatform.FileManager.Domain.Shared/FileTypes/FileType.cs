using System;

namespace SufiChain.SufiPlatform.FileManager.FileTypes;

/// <summary>
/// Represents the type of file content
/// </summary>
[Flags]
public enum FileType
{
    /// <summary>
    /// No file type specified
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Image files (jpg, png, gif, webp, etc.)
    /// </summary>
    Image = 1,
    
    /// <summary>
    /// Video files (mp4, webm, mov, etc.)
    /// </summary>
    Video = 2,
    
    /// <summary>
    /// Document files (pdf, doc, xls, etc.)
    /// </summary>
    Document = 4,
    
    /// <summary>
    /// Audio files (mp3, wav, etc.)
    /// </summary>
    Audio = 8
}
