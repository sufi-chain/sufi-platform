using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Account;

public class SufiAccountUrlOptions
{
    public string DefaultRootUrl { get; set; } = "https://localhost:44300";

    public Dictionary<string, string> AppRootUrls { get; set; } = new();
}
