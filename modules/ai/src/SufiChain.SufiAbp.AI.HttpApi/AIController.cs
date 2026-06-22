using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.AI;

public static class AIRemoteServiceConsts
{
    public const string ModuleName = "ai";
    public const string RemoteServiceName = "AI";
}

public abstract class AIController : SufiAbpControllerBase
{
}
