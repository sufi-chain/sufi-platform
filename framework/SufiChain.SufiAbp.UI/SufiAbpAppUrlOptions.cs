using System.Collections.Generic;

namespace SufiChain.SufiAbp.UI;

public class SufiAbpAppUrlOptions
{
    public Dictionary<string, SufiAbpApplicationUrlInfo> Applications { get; } = new();

    public List<string> RedirectAllowedUrls { get; } = new();

    public SufiAbpAppUrlOptions()
    {
        Applications["MVC"] = new SufiAbpApplicationUrlInfo();
    }
}

public class SufiAbpApplicationUrlInfo
{
    public string? RootUrl { get; set; }
}
