using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Permissions;

public class IsGrantedResponse
{
    public Guid UserId { get; set; }

    public Dictionary<string, bool> Permissions { get; set; }
}
