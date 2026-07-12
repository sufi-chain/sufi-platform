namespace SufiChain.SufiPlatform.CLI.ProjectBuilding;

/// <summary>
/// Represents a file entry in the template.
/// </summary>
public class FileEntry
{
    /// <summary>
    /// Relative path of the file.
    /// </summary>
    public required string RelativePath { get; set; }
    
    /// <summary>
    /// File content as bytes.
    /// </summary>
    public required byte[] Content { get; set; }
    
    /// <summary>
    /// Whether this is a binary file (should not be processed as text).
    /// </summary>
    public bool IsBinary { get; set; }
    
    /// <summary>
    /// Gets the file content as string (for text files).
    /// </summary>
    public string GetContentAsString() => System.Text.Encoding.UTF8.GetString(Content);
    
    /// <summary>
    /// Sets the file content from string.
    /// </summary>
    public void SetContent(string content) => Content = System.Text.Encoding.UTF8.GetBytes(content);
    
    /// <summary>
    /// Checks if the file is a text file based on extension.
    /// </summary>
    public static bool IsTextFile(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".cs" => true,
            ".razor" => true,
            ".cshtml" => true,
            ".csproj" => true,
            ".sln" => true,
            ".json" => true,
            ".xml" => true,
            ".config" => true,
            ".props" => true,
            ".targets" => true,
            ".md" => true,
            ".txt" => true,
            ".css" => true,
            ".scss" => true,
            ".js" => true,
            ".ts" => true,
            ".html" => true,
            ".htm" => true,
            ".yaml" => true,
            ".yml" => true,
            ".gitignore" => true,
            _ => false
        };
    }
}
