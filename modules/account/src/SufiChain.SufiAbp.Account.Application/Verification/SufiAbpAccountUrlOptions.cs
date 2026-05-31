using System.Collections.Generic;

namespace SufiChain.SufiAbp.Account;

public class SufiAbpAccountUrlOptions
{
    public string DefaultRootUrl { get; set; } = "https://localhost:44300";

    public Dictionary<string, string> AppRootUrls { get; set; } = new();
}
