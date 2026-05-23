namespace SufiChain.SufiAbp.FileManager.Plugins.Dtos;

public class ProcessingResult
{
    public byte[] Data { get; set; }
    public byte[] ThumbnailData { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

