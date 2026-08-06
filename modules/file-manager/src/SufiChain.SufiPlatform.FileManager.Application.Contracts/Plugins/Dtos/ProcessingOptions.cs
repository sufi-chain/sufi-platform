using System.Collections.Generic;

namespace SufiChain.SufiPlatform.FileManager.Plugins.Dtos;

public class ProcessingOptions
{
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public int Quality { get; set; } = 85;
    public bool GenerateThumbnail { get; set; }
    public int? ThumbnailWidth { get; set; }
    public int? ThumbnailHeight { get; set; }
    public Dictionary<string, object> AdditionalParameters { get; set; } = new();
}

